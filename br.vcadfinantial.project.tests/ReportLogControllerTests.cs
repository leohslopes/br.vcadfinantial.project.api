using br.vcadfinantial.project.api.Controllers.v1;
using br.vcadfinantial.project.api.Models.Requests;
using br.vcadfinantial.project.api.Models.Responses;
using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace br.vcadfinantial.project.tests
{
    public class ReportLogControllerTests
    {
        private readonly Mock<IReportLogService> _reportLogService;
        private readonly Mock<ILogger<ReportLogController>> _logger;
        private readonly ReportLogController _reportLogcontroller;

        public ReportLogControllerTests()
        {
            _reportLogService = new Mock<IReportLogService>();
            _logger = new Mock<ILogger<ReportLogController>>();
            _reportLogcontroller = new ReportLogController(_logger.Object, _reportLogService.Object);
        }

        [Fact]
        public async Task Download_ReturnsOk_WhenServiceSucceeds()
        {
            var request = new DownloadReportLogRequestModel { MonthKey = "2025-09" };
            var expectedData = "RelatorioGerado.xlsx";

            _reportLogService
                .Setup(s => s.Export(It.IsAny<ReportLogDTO>()))
                .ReturnsAsync(expectedData);

            var optionsMock = Mock.Of<IOptions<ApiBehaviorOptions>>();

            var result = await _reportLogcontroller.Download(request, optionsMock);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<StatusCode200TypedResponseModel<string>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(expectedData, response.Data);
        }

        [Fact]
        public async Task Download_ReturnsOkWithError_WhenServiceThrowsException()
        {
            var request = new DownloadReportLogRequestModel { MonthKey = "2025-09" };

            _reportLogService
                .Setup(s => s.Export(It.IsAny<ReportLogDTO>()))
                .ThrowsAsync(new Exception("Erro ao exportar relatório"));

            var optionsMock = Mock.Of<IOptions<ApiBehaviorOptions>>();

            var result = await _reportLogcontroller.Download(request, optionsMock);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<StatusCode200StandardResponseModel>(okResult.Value);

            Assert.False(response.Success);
            Assert.Contains(response.Errors, e => e.Value == "Erro ao exportar relatório");
        }

    }
}
