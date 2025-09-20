using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Entities.Login;
using br.vcadfinantial.project.domain.Entities.Tables;
using br.vcadfinantial.project.domain.Interfaces.Repositories;
using br.vcadfinantial.project.domain.Interfaces.Services;
using br.vcadfinantial.project.repository.Database;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.application.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IPasswordResetRepository _passwordResetRepository;

        public UserService(ILogger<UserService> logger, 
            IUserRepository userRepository, 
            AppDbContext context, 
            IPasswordHasher<User> passwordHasher, 
            IConfiguration configuration, 
            IPasswordResetRepository passwordResetRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _passwordResetRepository = passwordResetRepository;
        }

        public async Task<bool> CreateUser(UserDTO dto)
        {
            bool result = false;
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var passwordHasher = new PasswordHasher<UserDTO>();

                var user = new User
                {
                    FullName = dto.FullName,
                    Gender = dto.Gender,
                    Email = dto.Email,
                    Password = passwordHasher.HashPassword(dto, dto.Password),
                    Photo = await GetImageBytes(dto.Photo),
                    CreateDate = dto.CreateDate
                };

                var existingUser = await _userRepository.GetByEmail(dto.Email);

                if (existingUser != null && existingUser.Email.ToUpper().Trim().Equals(dto.Email.ToUpper().Trim()))
                {
                    _logger.LogWarning($"Já existe o e-mail {dto.Email.ToUpper().Trim()} cadastrado.");
                    throw new ApplicationException($"Já existe o e-mail {dto.Email.ToUpper().Trim()} cadastrado.");
                }

                _logger.LogInformation($"Inserindo o usuário {dto.FullName.ToUpper().Trim()} no banco de dados.");
                await _userRepository.AddAsync(user);
                _logger.LogInformation($"Registro do usuário {dto.FullName.ToUpper().Trim()} feito com sucesso no banco de dados.");

                await transaction.CommitAsync();
                result = true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"[CreateUser] - Erro ao cadastrar o usuário: {ex.Message}");
                throw;
            }

            return result;
        }

        public async Task<UserSession> GetToken(LoginDTO dto)
        {
            UserSession result = new();

            try
            {
                var existingUser = await _userRepository.GetByEmail(dto.Email);

                if (existingUser == null)
                {
                    _logger.LogWarning("Credenciais inválidas.");
                    throw new ApplicationException($"Credenciais inválidas.");
                }

                var resultAuth = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, dto.Password);

                if (resultAuth.Equals(PasswordVerificationResult.Failed))
                {
                    _logger.LogWarning("Credenciais inválidas.");
                    throw new ApplicationException($"Credenciais inválidas.");
                }

                result.Token = GenerateJwtToken(existingUser);
                result.User = existingUser;


            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetToken] - Erro ao gerar o token pro login: {ex.Message}");
                throw;
            }

            return result;

        }

        public async Task<bool> UpdateUser(UserDTO dto)
        {
            bool result = false;
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var passwordHasher = new PasswordHasher<UserDTO>();

                var user = new User
                {
                    ID = dto.Id,
                    FullName = dto.FullName,
                    Gender = dto.Gender,
                    Email = dto.Email,
                    Password = passwordHasher.HashPassword(dto, dto.Password),
                    Photo = await GetImageBytes(dto.Photo),
                    CreateDate = dto.CreateDate
                };

                _logger.LogInformation($"Atualizando o usuário {dto.FullName.ToUpper().Trim()} no banco de dados.");
                await _userRepository.UpdateAsync(user);
                _logger.LogInformation($"Registro do usuário {dto.FullName.ToUpper().Trim()} atualizado com sucesso no banco de dados.");

                await transaction.CommitAsync();
                result = true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"[UpdateUser] - Erro ao atualizar o usuário: {ex.Message}");
                throw new Exception();
            }

            return result;
        }

        public async Task<string> SendResetCode(PasswordResetDTO dto)
        {
            string result = string.Empty;

            try
            {
                var existingUser = await _userRepository.GetByEmail(dto.Email);
                if (existingUser == null)
                {
                    _logger.LogWarning("E-mail não encontrado.");
                    throw new ApplicationException($"E-mail não encontrado.");
                }

                var code = new Random().Next(100000, 999999).ToString();

                _logger.LogInformation($"Fazendo o envio do código ao e-mail {dto.Email}.");
                var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
                {
                    Port = int.Parse(_configuration["Smtp:Port"]),
                    Credentials = new NetworkCredential(_configuration["Smtp:User"], _configuration["Smtp:Pass"]),
                    EnableSsl = true
                };

                string htmlBody = BuildEmailBody(existingUser.FullName, code);

                var mail = new MailMessage
                {
                    From = new MailAddress(_configuration["Smtp:From"]),
                    Subject = "Código de Recuperação de Senha - VCADFINANTIAL",
                    IsBodyHtml = true,
                };
                mail.To.Add(dto.Email);

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");

                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo-vcadfinantial.png");
                LinkedResource logo = new(logoPath, "image/png")
                {
                    ContentId = "LogoVcad"
                };
                htmlView.LinkedResources.Add(logo);

                mail.AlternateViews.Add(htmlView);

                await smtpClient.SendMailAsync(mail);
                _logger.LogInformation($"Envio do código ao e-mail {dto.Email} feito com sucesso.");

                _logger.LogInformation($"Inserindo o código do e-mail {dto.Email} na base.");
                var passwordReset = new PasswordReset
                {
                    Email = dto.Email,
                    ResetCode = code,
                    DateExpire = DateTime.UtcNow.AddMinutes(15)
                };

                var existingCode = await _context.PasswordReset.FirstOrDefaultAsync(x => x.Email.Equals(dto.Email));
                if (existingCode == null)
                {
                    await _passwordResetRepository.AddAsync(passwordReset);
                }
                else
                {
                    existingCode.ResetCode = code;
                    existingCode.DateExpire = DateTime.UtcNow.AddMinutes(15);
                    await _passwordResetRepository.UpdateAsync(existingCode);
                }
                _logger.LogInformation($"Código do e-mail {dto.Email} inserido na base com sucesso.");

                result = code;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SendResetCode] - Erro ao enviar código para o e-mail {dto.Email}: {ex.Message}");
                throw;
            }

            return result;
        }

        public async Task<bool> ConfirmPassword(ConfimPasswordDTO dto)
        {
            bool result = false;
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingUser = await _userRepository.GetByEmail(dto.Email);
                if (existingUser == null)
                {
                    _logger.LogWarning("E-mail não encontrado.");
                    throw new ApplicationException($"E-mail não encontrado.");
                }

                var passwordHasher = new PasswordHasher<ConfimPasswordDTO>();
                existingUser.Password = passwordHasher.HashPassword(dto, dto.Password);

                _logger.LogInformation($"Atualizando a senha do usuário {existingUser.FullName.ToUpper().Trim()} no banco de dados.");
                await _userRepository.UpdateAsync(existingUser); 
                _logger.LogInformation($"Atualização da senha do usuário {existingUser.FullName.ToUpper().Trim()} feito com sucesso no banco de dados.");

                await transaction.CommitAsync();
                result = true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"[ConfirmPassword] - Erro ao confirmar senha para o e-mail {dto.Email}: {ex.Message}");
                throw;
            }

            return result;
        }

        private static async Task<byte[]?> GetImageBytes(IFormFile? picture)
        {
            byte[]? imgBytes = null;

            if (picture is not null)
            {
                using var ms = new MemoryStream();
                await picture.CopyToAsync(ms);
                imgBytes = ms.ToArray();
            }

            return imgBytes;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.ID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string BuildEmailBody(string userName, string code)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='pt-BR'>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='UTF-8'>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, Helvetica, sans-serif; background-color: #ffffff; margin: 0; padding: 20px; color: #333333; }");
            sb.AppendLine("        .logo { text-align: center; margin-bottom: 20px; }");
            sb.AppendLine("        .card { max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; }");
            sb.AppendLine("        h2 { color: #004d66; }");
            sb.AppendLine("        .code { font-size: 28px; font-weight: bold; color: #009999; margin: 20px 0; text-align: center; }");
            sb.AppendLine("        p { font-size: 16px; line-height: 1.5; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class='logo'>");
            sb.AppendLine("        <img src='cid:LogoVcad' alt='VCADFINANTIAL' style='max-width:200px;'/>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    <div class='card'>");
            sb.AppendLine("        <h2>Seu código de acesso temporário</h2>");
            sb.AppendLine($"        <p>Olá, {userName}.</p>");
            sb.AppendLine("        <p>Recebemos uma solicitação para redefinição de senha da sua conta.</p>");
            sb.AppendLine("        <p>Use o código abaixo para prosseguir:</p>");
            sb.AppendLine($"        <div class='code'>{code}</div>");
            sb.AppendLine("        <p>Este código expira em <strong>15 minutos</strong>. Se você não fez essa solicitação, pode ignorar este e-mail.</p>");
            sb.AppendLine("        <p style='font-size:12px;color:#777;'>Não compartilhe este código com ninguém.</p>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

    }
}
