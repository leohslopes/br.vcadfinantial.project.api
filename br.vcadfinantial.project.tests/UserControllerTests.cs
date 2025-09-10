using br.vcadfinantial.project.api.Controllers.v1;
using br.vcadfinantial.project.api.Models.Requests;
using br.vcadfinantial.project.api.Models.Responses;
using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Entities.Login;
using br.vcadfinantial.project.domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace br.vcadfinantial.project.tests;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userService;
    private readonly Mock<ILogger<UserController>> _logger;
    private readonly UserController _userController;
    private readonly IOptions<ApiBehaviorOptions> _options;

    public UserControllerTests()
    {
        _userService = new Mock<IUserService>();
        _logger = new Mock<ILogger<UserController>>();
        _options = Options.Create(new ApiBehaviorOptions());
        _userController = new UserController(_logger.Object, _userService.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenUserCreated()
    {
        var request = new RegisterUserRequestModel
        {
            FullName = "Teste User",
            Gender = "M",
            Email = "teste@teste.com",
            Password = "123456",
            Photo = null
        };

        _userService
            .Setup(s => s.CreateUser(It.IsAny<UserDTO>()))
            .ReturnsAsync(true);

        var result = await _userController.Register(request, _options);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<StatusCode200TypedResponseModel<bool>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUserNotCreated()
    {
        var request = new RegisterUserRequestModel
        {
            FullName = "Teste User",
            Gender = "M",
            Email = "teste@teste.com",
            Password = "123456",
            Photo = null
        };

       _userService
            .Setup(s => s.CreateUser(It.IsAny<UserDTO>()))
            .ReturnsAsync(false);

        var result = await _userController.Register(request, _options);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<StatusCode200TypedResponseModel<bool>>(badRequest.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var request = new LoginRequestModel
        {
            Email = "teste@teste.com",
            Password = "123456"
        };

        var fakeSession = new UserSession { Token = "fake-token" };

        _userService
            .Setup(s => s.GetToken(It.IsAny<LoginDTO>()))
            .ReturnsAsync(fakeSession);

        var result = await _userController.Login(request, _options);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<StatusCode200TypedResponseModel<UserSession>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("fake-token", response.Data.Token);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenIdsDoNotMatch()
    {
        var request = new UpdateUserRequestModel
        {
            Id = 2,
            FullName = "Teste User",
            Gender = "M",
            Email = "teste@teste.com",
            Password = "123456",
            Photo = null
        };

        var result = await _userController.Update(1, request, _options);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("O ID do usuário não corresponde ao ID fornecido.", badRequest.Value);
    }

    [Fact]
    public async Task Forgot_ShouldReturnOk_WhenEmailIsSent()
    {
        var request = new ForgotEmailRequestModel { Email = "teste@teste.com" };

        _userService
            .Setup(s => s.SendResetCode(It.IsAny<PasswordResetDTO>()))
            .ReturnsAsync("reset-code");

        var result = await _userController.Forgot(request, _options);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<StatusCode200TypedResponseModel<string>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("reset-code", response.Data);
    }

    [Fact]
    public async Task Confirm_ShouldReturnOk_WhenPasswordConfirmed()
    {
        var request = new ConfirmCodePasswordRequestModel
        {
            Email = "teste@teste.com",
            Password = "123456",
            Code = "1234"
        };

        _userService
            .Setup(s => s.ConfirmPassword(It.IsAny<ConfimPasswordDTO>()))
            .ReturnsAsync(true);

        var result = await _userController.Confirm(request, _options);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<StatusCode200TypedResponseModel<bool>>(okResult.Value);
        Assert.True(response.Success);
    }
}
