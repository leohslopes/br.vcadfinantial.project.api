using br.vcadfinantial.project.api.Controllers.v1;
using br.vcadfinantial.project.api.Models.Responses;
using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace br.vcadfinantial.project.tests
{
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _accountService;
        private readonly Mock<ILogger<AccountController>> _logger;
        private readonly AccountController _accountController;
        private readonly IOptions<ApiBehaviorOptions> _options;

        public AccountControllerTests()
        {
            _accountService = new Mock<IAccountService>();
            _logger = new Mock<ILogger<AccountController>>();
            _options = Options.Create(new ApiBehaviorOptions());
            _accountController = new AccountController(_logger.Object, _accountService.Object);
        }

        [Fact]
        public async Task Get_ReturnsOk_WithData()
        {
            var fakeData = new List<DocumentAccountInfoAgreggate>
                           {
                              new DocumentAccountInfoAgreggate
                              {
                                MounthKey = "01/2025",
                                FileName = "arquivo1.xml",
                                OfficialNumber = "123456",
                              },
                              new DocumentAccountInfoAgreggate
                              {
                                 MounthKey = "02/2025",
                                 FileName = "arquivo2.xml",
                                 OfficialNumber = "654321",
                              }
                            };

            _accountService
                .Setup(s => s.GetAll())
                .ReturnsAsync(fakeData);

            var result = await _accountController.Get(_options);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var response = Assert.IsType<StatusCode200TypedResponseModel<IEnumerable<DocumentAccountInfoAgreggate>>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(2, response.Data.Count());
        }

        [Fact]
        public async Task Get_ReturnsOk_WhenExceptionThrown()
        {
            _accountService
                .Setup(s => s.GetAll())
                .ThrowsAsync(new Exception("Erro de teste"));

            var result = await _accountController.Get(_options);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<StatusCode200StandardResponseModel>(okResult.Value);

            Assert.False(response.Success);
            Assert.Contains(response.Errors, e => e.Value == "Erro de teste");
        }

    }
}