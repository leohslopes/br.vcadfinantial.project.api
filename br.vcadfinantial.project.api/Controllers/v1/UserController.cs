using br.vcadfinantial.project.api.Models.Requests;
using br.vcadfinantial.project.api.Models.Responses;
using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Entities.Login;
using br.vcadfinantial.project.domain.Interfaces.Services;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace br.vcadfinantial.project.api.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService archiveService)
        {
            _logger = logger;
            _userService = archiveService;
        }

        [HttpPost("Register"), MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromForm] RegisterUserRequestModel requestModel, IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            try
            {
                var dto = new UserDTO(0, requestModel.FullName, requestModel.Gender, requestModel.Email, requestModel.Password, requestModel.Photo, DateTime.UtcNow);
                var resultAsync = await _userService.CreateUser(dto);

                if (!resultAsync)
                {
                    return BadRequest(new StatusCode200TypedResponseModel<bool>()
                    {
                        Success = resultAsync
                    });
                }

                return Ok(new StatusCode200TypedResponseModel<bool>()
                {
                    Success = resultAsync
                });
            }
            catch (Exception ex)
            {
                var rt = new StatusCode200StandardResponseModel
                {
                    Success = false
                };
                rt.Errors.Add(new KeyValuePair<string, string>("error", ex.Message));
                return Ok(rt);
            }
        }

        [HttpPost("Login"), MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel requestModel, IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            try
            {
                var dto = new LoginDTO(requestModel.Email, requestModel.Password);
                var resultAsync = await _userService.GetToken(dto);

           
                return Ok(new StatusCode200TypedResponseModel<UserSession>()
                {
                    Success = true,
                    Data = resultAsync
                });
            }
            catch (Exception ex)
            {
                var rt = new StatusCode200StandardResponseModel
                {
                    Success = false
                };
                rt.Errors.Add(new KeyValuePair<string, string>("error", ex.Message));
                return Ok(rt);
            }
        }

        [HttpPut("{id}"), MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateUserRequestModel requestModel, IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            try
            {
                if (!id.Equals(requestModel.Id))
                {
                    return BadRequest("O ID do usuário não corresponde ao ID fornecido.");
                }

                var dto = new UserDTO(requestModel.Id,requestModel.FullName, requestModel.Gender, requestModel.Email, requestModel.Password, requestModel.Photo, DateTime.UtcNow);
                var resultAsync = await _userService.UpdateUser(dto);

                if (!resultAsync)
                {
                    return BadRequest(new StatusCode200TypedResponseModel<bool>()
                    {
                        Success = resultAsync
                    });
                }

                return Ok(new StatusCode200TypedResponseModel<bool>()
                {
                    Success = resultAsync
                });
            }
            catch (Exception ex)
            {
                var rt = new StatusCode200StandardResponseModel
                {
                    Success = false
                };
                rt.Errors.Add(new KeyValuePair<string, string>("error", ex.Message));
                return Ok(rt);
            }
        }


    }
}
