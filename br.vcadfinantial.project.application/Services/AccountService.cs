using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Entities.Archive;
using br.vcadfinantial.project.domain.Entities.Tables;
using br.vcadfinantial.project.domain.Enumerations;
using br.vcadfinantial.project.domain.Exceptions;
using br.vcadfinantial.project.domain.Interfaces.Repositories;
using br.vcadfinantial.project.domain.Interfaces.Services;
using br.vcadfinantial.project.repository.Database;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Xml.Linq;

namespace br.vcadfinantial.project.application.Services
{
    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly IAccountRepository _accountRepository; 
        private readonly IDocumentRepository _documentRepository;
        private readonly AppDbContext _context;

        public AccountService(ILogger<AccountService> logger, IDocumentRepository documentRepository, IAccountRepository accountRepository, AppDbContext context)
        {
            _logger = logger;
            _documentRepository = documentRepository;   
            _accountRepository = accountRepository;
            _context = context;
        }

        public async Task<ResultSetImportArchive> ImportFile(DocumentDTO dto)
        {
            ResultSetImportArchive result = new();
            using var stream = dto.File.OpenReadStream();
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            { 
                var doc = XDocument.Load(stream);

                _logger.LogInformation("Validando o layout do XML.");
                var root = doc.Root;
                if (root is null ||
                    root.Attribute("codigoDocumento") is null ||
                    root.Attribute("cnpj") is null ||
                    root.Attribute("dataBase") is null ||
                    root.Attribute("tipoRemessa") is null)
                {
                    throw new InvalidDataException("Layout inválido: atributos obrigatórios ausentes no nó raiz do XML.");
                }

                var contas = doc.Descendants("conta").ToList();
                if (contas.Count == 0)
                {
                    throw new InvalidDataException("Layout inválido: nenhum nó <conta> encontrado no XML.");
                }

                foreach (var conta in contas)
                {
                    if (conta.Attribute("codigoConta") is null || conta.Attribute("saldo") is null)
                    {
                        throw new InvalidDataException("Layout inválido: cada nó <conta> deve conter os atributos 'codigoConta' e 'saldo'.");
                    }
                }

                int documentCode = Convert.ToInt32((string)root.Attribute("codigoDocumento") ?? "0");
                if (!documentCode.Equals(DocumentType.GeneralAccount.Id))
                {
                    throw new InvalidDataException($"Layout inválido: o tipo de documento {documentCode} não é suportado. Esperado: {DocumentType.GeneralAccount.Id}.");
                }
                _logger.LogInformation("Validação do layout no XML feita com sucesso.");

                _logger.LogInformation("Consumindo o arquivo XML de contas.");
                var document = new ImportDocument
                {
                    DocumentCode = documentCode,
                    OfficialNumber = (string)doc.Root?.Attribute("cnpj") ?? string.Empty,
                    MounthKey = (string)doc.Root?.Attribute("dataBase") ?? string.Empty,
                    ShipmentType = (string)doc.Root?.Attribute("tipoRemessa") ?? string.Empty,
                    Accounts = [.. doc.Descendants("conta").Select(static x => new ImportAccount
                    {
                        AccountKey = Convert.ToInt64((string)x.Attribute("codigoConta") ?? "0"),
                        Among = Convert.ToDecimal((string)x.Attribute("saldo") ?? "0", CultureInfo.InvariantCulture)
                    })]
                };

                var existing = await _context.Document.FirstOrDefaultAsync(x => x.MounthKey.Equals(document.MounthKey));
                if (existing != null && !dto.Overwrite)
                {
                    _logger.LogWarning($"Já existe um arquivo vigente para o código base {existing.MounthKey}. Arquivo recebido: {existing.FileName}");
                    throw new FileAlreadyExistsException(existing.MounthKey, existing.FileName);
                }
                else if(dto.Overwrite) 
                {
                    _logger.LogInformation($"Removendo o histórico do arquivo passado. Mês base{existing.MounthKey}");
                    await _accountRepository.DeleteAsync(existing.IdDocument);
                    await _documentRepository.DeleteAsync(existing.IdDocument);
                    _logger.LogInformation($"Histórico do arquivo passado removido com sucesso. Mês base{existing.MounthKey}");

                }
                _logger.LogInformation("Consumo do arquivo XML de contas finalizado com sucesso.");

                _logger.LogInformation("Inativando arquivos vigentes anteriores.");
                await _documentRepository.InactivateDocumentsByMonth();
                _logger.LogInformation("Arquivos vigentes anteriores inativos com sucesso");

                _logger.LogInformation("Inserindo registros no banco de dados.");
                var tbDoc = new Document
                {
                    DocumentCode = document.DocumentCode,
                    OfficialNumber = document.OfficialNumber,
                    MounthKey = document.MounthKey,
                    ShipmentType = document.ShipmentType,
                    FileName = dto.File.FileName,
                    CreatedByUserId = dto.UserId,
                    Active = true,
                    CreateDate = DateTime.UtcNow
                };
                
                await _documentRepository.AddAsync(tbDoc);

                foreach (var account in document.Accounts)
                {
                    var item = new Account
                    {
                        IdDocument = tbDoc.IdDocument,
                        AccountKey = account.AccountKey,
                        Among = account.Among,
                        Active = true,
                        CreateDate = DateTime.UtcNow
                    };

                    await _accountRepository.AddAsync(item);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Inserção concluída com sucesso no banco de dados.");

                _logger.LogInformation("Gerando a planilha de retorno do input massivo.");
                var fileBytes = GenerateFinalResultsArchive(document, tbDoc).FileContents;
                var base64 = Convert.ToBase64String(fileBytes);

                result.ResultFileContent = base64;
                result.CountRows = document.Accounts.Count;
                _logger.LogInformation("Planilha de retorno do input massivo gerado com sucesso.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"[ImportFile] - Erro ao importar o arquivo XML de conta: {ex.Message}");
                throw;
            }

            return result;
        }

        public async Task<IEnumerable<DocumentAccountInfoAgreggate>> GetAll(AccountDTO dto)
        {
            IEnumerable<DocumentAccountInfoAgreggate> result;

            try
            {
                result = await _documentRepository.GetAll(dto.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetAll] - Erro fazer a consulta das contas: {ex.Message}");
                throw new Exception();
            }

            return result;
        }

        public async Task<IEnumerable<DocumentAccountInfoAgreggate>> GetByAccountKey(AccountDTO dto)
        {
            IEnumerable<DocumentAccountInfoAgreggate> result;

            try
            {
                result = await _documentRepository.GetByAccountKey(dto.AccountKey, dto.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetByAccountKey] - Erro fazer a consulta por filtro do número da conta: {ex.Message}");
                throw new Exception();
            }

            return result;
        }

        public async Task<bool> InactiveAccount(AccountDTO dto)
        {
            bool result = false;
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Validando usuário no banco de dados.");

                var existingUser = await _context.User.FirstOrDefaultAsync(x => x.ID.Equals(dto.UserId));
                if (existingUser == null)
                {
                    _logger.LogWarning("Usuário não encontrado.");
                    throw new ApplicationException($"Usuário não encontrado.");
                }
                _logger.LogInformation("Usuário no banco de dados validado com sucesso.");

                _logger.LogInformation($"Inativando o documento atrelado ao usuário {existingUser?.FullName.ToUpper().Trim()}.");

                var document = await _context.Document.FirstOrDefaultAsync(x => x.CreatedByUserId.Equals(dto.UserId) && x.Active);
                if (document == null)
                {
                    _logger.LogWarning("Documento não encontrado.");
                    throw new ApplicationException($"Documento não encontrado.");
                }

                document.Active = false;
                await _documentRepository.UpdateAsync(document);

                var accounts = await _context.Account.Where(x => x.IdDocument.Equals(document.IdDocument) && x.Active).ToListAsync();
                if (accounts == null)
                {
                    _logger.LogWarning("Conta(s) não encontrada(s).");
                    throw new ApplicationException($"Conta(s) não encontrada(s).");
                }

                foreach (var account in accounts)
                {
                    account.Active = false;
                    await _accountRepository.UpdateAsync(account);
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Documento atrelado ao usuário {existingUser?.FullName.ToUpper().Trim()} inativado com sucesso.");

                result = true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"[InactiveAccount] - Erro ao inativar a(s) conta(s): {ex.Message}");
                throw new Exception();
            }

            return result;
        }

        private static FileContentResult GenerateFinalResultsArchive(ImportDocument document, Document tbDoc)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Resultado");

            worksheet.Cell(1, 1).Value = "CÓDIGO DOCUMENTO";
            worksheet.Cell(1, 2).Value = "CNPJ";
            worksheet.Cell(1, 3).Value = "MÊS";
            worksheet.Cell(1, 4).Value = "REMESSA";
            worksheet.Cell(1, 5).Value = "NÚMERO CONTA";
            worksheet.Cell(1, 6).Value = "SALDO";

            var row = 2;
            foreach (var acc in document.Accounts)
            {
                worksheet.Cell(row, 1).Value = tbDoc.DocumentCode;
                worksheet.Cell(row, 2).Value = tbDoc.OfficialNumber;
                worksheet.Cell(row, 3).Value = tbDoc.MounthKey;
                worksheet.Cell(row, 4).Value = tbDoc.ShipmentType;
                worksheet.Cell(row, 5).Value = acc.AccountKey;
                worksheet.Cell(row, 6).Value = acc.Among;
                row++;
            }

            int totalRows = document.Accounts.Count + 1;
            var tableRange = worksheet.Range(1, 1, totalRows, 6);
            var table = tableRange.CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium9;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new FileContentResult(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "resultado-importacao.xlsx"
            };
        }
    }
}
