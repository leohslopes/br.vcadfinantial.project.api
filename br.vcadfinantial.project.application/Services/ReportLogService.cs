using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Interfaces.Repositories;
using br.vcadfinantial.project.domain.Interfaces.Services;
using br.vcadfinantial.project.repository.Database;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.application.Services
{
    public class ReportLogService : IReportLogService
    {
        private readonly ILogger<ReportLogService> _logger;
        private readonly IDocumentRepository _documentRepository;

        public ReportLogService(ILogger<ReportLogService> logger, IDocumentRepository documentRepository)
        {
            _logger = logger;
            _documentRepository = documentRepository;
        }

        public async Task<string> Export(ReportLogDTO dto)
        {
            string download = string.Empty;

            try
            {
                var datas = await _documentRepository.GetReport(dto.MonthKey, dto.UserId);

                if (datas != null && datas.Count() == 0)
                {
                    _logger.LogWarning($"Dados não encontrados.");
                    throw new ApplicationException($"Dados não encontrados.");
                }

                _logger.LogInformation($"Gerando o relatório de log para o mês {dto.MonthKey}.");
                var report = GenerateLayoutReportLog([.. datas.OrderBy(x => x.AccountKey)]).FileContents;
                download = Convert.ToBase64String(report);
                _logger.LogInformation($"Relatório de log para o mês {dto.MonthKey} gerado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Export] - Erro ao baixar o relatório de log para o filtro {dto.MonthKey}: {ex.Message}");
                throw new Exception();

            }

            return download;
        }

        private static FileContentResult GenerateLayoutReportLog(List<ReportLogInfoAgreggate> logs)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("RESULTADO");

            var turquoise = XLColor.FromHtml("#50C1BF");
            var petroleum = XLColor.FromHtml("#197D88");

            worksheet.Cell("B2").Value = "RELATÓRIO LOG";
            worksheet.Cell("B2").Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Font.SetFontColor(petroleum);

            worksheet.Cell("A4").Value = "MÊS:";
            worksheet.Cell("B4").Value = logs[0].MounthKey;

            worksheet.Cell("C4").Value = "CNPJ:";
            worksheet.Cell("D4").Value = logs[0].OfficialNumber;

            worksheet.Cell("A5").Value = "ARQUIVO:";
            worksheet.Cell("B5").Value = logs[0].FileName;

            worksheet.Cell("C5").Value = "STATUS:";
            worksheet.Cell("D5").Value = logs[0].Active ? "Ativo" : "Inativo";

            worksheet.Range("A4:A5").Style.Font.SetBold();
            worksheet.Range("C4:C5").Style.Font.SetBold();

            var guid = Guid.NewGuid();
            worksheet.Cell("A6").Value = "GUID:";
            worksheet.Cell("B6").Value = guid.ToString();
            worksheet.Cell("A6").Style.Font.SetBold();

            worksheet.Cell("A8").Value = "CONTA";
            worksheet.Cell("B8").Value = "SALDO";

            worksheet.Range("A8:B8").Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(turquoise)
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            int row = 9;
            foreach (var log in logs)
            {
                worksheet.Cell(row, 1).Value = log.AccountKey;
                worksheet.Cell(row, 2).Value = log.Among;
                worksheet.Cell(row, 2).Style.NumberFormat.SetFormat("R$ #,##0.00");
                row++;
            }

            worksheet.Range($"A8:B{row - 1}").Style
                .Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range($"A8:B{row - 1}").Style
                .Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new FileContentResult(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{guid}.xlsx"
            };

        }
    }
}
