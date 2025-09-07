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
                    _logger.LogWarning("Credenciais inválidas!");
                    throw new ApplicationException($"Credenciais inválidas!");
                }

                var resultAuth = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, dto.Password);

                if (resultAuth.Equals(PasswordVerificationResult.Failed))
                {
                    _logger.LogWarning("Credenciais inválidas!");
                    throw new ApplicationException($"Credenciais inválidas!");
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
                _logger.LogError($"[CreateUser] - Erro ao cadastrar o usuário: {ex.Message}");
                throw;
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

                var mail = new MailMessage
                {
                    From = new MailAddress(_configuration["Smtp:From"]),
                    Subject = "Código de Recuperação de Senha",
                    Body = $"Seu código de recuperação é: {code}",
                    IsBodyHtml = false,
                };

                mail.To.Add(dto.Email);
                await smtpClient.SendMailAsync(mail);
                _logger.LogInformation($"Envio do código ao e-mail {dto.Email} feito com sucesso.");

                _logger.LogInformation($"Inserindo o código do e-mail {dto.Email} na base.");
                var passwordReset = new PasswordReset {
                    Email = dto.Email,
                    ResetCode = code,
                    DateExpire = DateTime.UtcNow.AddMinutes(15)
                };
                await _passwordResetRepository.AddAsync(passwordReset);
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

                var passwordReset = await _context.PasswordReset.Where(x => x.Email.Equals(dto.Email)).FirstOrDefaultAsync();

                if (passwordReset != null && (!passwordReset.ResetCode.Equals(dto.Code) || passwordReset.DateExpire < DateTime.UtcNow))
                {
                    _logger.LogWarning("Código inválido ou expirado.");
                    throw new ApplicationException($"Código inválido ou expirado.");
                }

                var passwordHasher = new PasswordHasher<ConfimPasswordDTO>();
                var user = new User
                {
                    ID = existingUser.ID,
                    FullName = existingUser.FullName,
                    Gender = existingUser.Gender,
                    Email = existingUser.Email,
                    Password = passwordHasher.HashPassword(dto, dto.Password),
                    Photo = existingUser.Photo,
                    CreateDate = DateTime.UtcNow
                };

                _logger.LogInformation($"Atualizando a senha do usuário {existingUser.FullName.ToUpper().Trim()} no banco de dados.");
                await _userRepository.UpdateAsync(user);
                _logger.LogInformation($"Atualização da senha {existingUser.FullName.ToUpper().Trim()} feito com sucesso no banco de dados.");

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

    }
}
