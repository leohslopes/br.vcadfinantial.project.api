using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Entities.Archive;
using br.vcadfinantial.project.domain.Entities.Tables;
using br.vcadfinantial.project.domain.Interfaces.Repositories;
using br.vcadfinantial.project.domain.Interfaces.Services;
using br.vcadfinantial.project.repository.Database;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
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

                _logger.LogInformation("Consumindo o arquivo XML de contas.");
                var document = new ImportDocument
                {
                    DocumentCode = Convert.ToInt32((string)doc.Root?.Attribute("codigoDocumento") ?? "0"),
                    OfficialNumber = (string)doc.Root?.Attribute("cnpj") ?? string.Empty,
                    MounthKey = (string)doc.Root?.Attribute("dataBase") ?? string.Empty,
                    ShipmentType = (string)doc.Root?.Attribute("tipoRemessa") ?? string.Empty,
                    Accounts = [.. doc.Descendants("conta").Select(static x => new ImportAccount
                    {
                        AccountKey = Convert.ToInt64((string)x.Attribute("codigoConta") ?? "0"),
                        Among = Convert.ToDecimal((string)x.Attribute("saldo") ?? "0", CultureInfo.InvariantCulture)
                    })]
                };

                var existing = await _context.Document.FirstOrDefaultAsync();
                if (existing != null && existing.MounthKey.Equals(document.MounthKey))
                {
                    _logger.LogWarning($"Já existe um arquivo vigente para o código base {existing.MounthKey}. Arquivo recebido: {existing.FileName}");
                    throw new InvalidOperationException($"Já existe um arquivo vigente para o código base {existing.MounthKey}. Arquivo recebido: {existing.FileName}");
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
                    Active = true
                };
                
                await _documentRepository.AddAsync(tbDoc);

                foreach (var account in document.Accounts)
                {
                    var item = new Account
                    {
                        DocumentCode = tbDoc.DocumentCode,
                        AccountKey = account.AccountKey,
                        Among = account.Among,
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

        public async Task<IEnumerable<DocumentAccountInfoAgreggate>> GetAll()
        {
            IEnumerable<DocumentAccountInfoAgreggate> result;

            try
            {
                result = await _context.Account
                         .Where(x => x.Document.Active)
                         .Select(y => new DocumentAccountInfoAgreggate
                         {
                             MounthKey = y.Document!.MounthKey,
                             FileName = y.Document.FileName,
                             OfficialNumber = y.Document.OfficialNumber,
                             Among = y.Among,
                             AccountKey = y.AccountKey
                         })
                         .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetAll] - Erro fazer a consulta das contas: {ex.Message}");
                throw;
            }

            return result;
        }

        public async Task<IEnumerable<DocumentAccountInfoAgreggate>> GetByAccountKey(AccountDTO dto)
        {
            IEnumerable<DocumentAccountInfoAgreggate> result;

            try
            {
                result = await _context.Account
                         .Where(x => x.Document.Active && x.AccountKey.Equals(dto.AccountKey))
                         .Select(y => new DocumentAccountInfoAgreggate
                         {
                             MounthKey = y.Document!.MounthKey,
                             FileName = y.Document.FileName,
                             OfficialNumber = y.Document.OfficialNumber,
                             Among = y.Among,
                             AccountKey = y.AccountKey
                         })
                         .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetAll] - Erro fazer a consulta das contas: {ex.Message}");
                throw;
            }

            return result;
        }

        private FileContentResult GenerateFinalResultsArchive(ImportDocument document, Document tbDoc)
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
            var tableRange = worksheet.Range(1, 1, totalRows, 7);
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
