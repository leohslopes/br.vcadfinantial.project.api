using br.vcadfinantial.project.api.Models.Requests;
using br.vcadfinantial.project.api.Models.Responses;
using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Entities.Archive;
using br.vcadfinantial.project.domain.Exceptions;
using br.vcadfinantial.project.domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace br.vcadfinantial.project.api.Controllers.v1
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _archiveService;

        public AccountController(ILogger<AccountController> logger, IAccountService archiveService)
        {
            _logger = logger;
            _archiveService = archiveService;
        }

        [HttpPost("Import/{force:bool}"), MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Import(bool force, [FromForm] ImportFileRequestModel requestModel, IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            try
            {
                if(requestModel.File == null)
                {
                    BadRequest("Arquivo não encontrado.");
                }

                var dto = new DocumentDTO(requestModel.File, force, int.Parse(requestModel.UserId));
                var resultAsync = await _archiveService.ImportFile(dto);

                return Ok(new StatusCode200TypedResponseModel<ResultSetImportArchive>()
                {
                    Success = true,
                    Data = resultAsync
                });
            }
            catch (FileAlreadyExistsException ex)
            {
                var rt = new StatusCode200StandardResponseModel
                {
                    Success = false
                };
                rt.Errors.Add(new KeyValuePair<string, string>("error", ex.Message));

                return Conflict(rt);
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

        [HttpGet("GetAll/filter-search"), MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] GetAllAccountRequestModel requestModel, IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            try
            {
                var dto = new AccountDTO(requestModel.AccountKey, int.Parse(requestModel.UserId));
                var resultAsync = await _archiveService.GetAll(dto);

                return Ok(new StatusCode200TypedResponseModel<IEnumerable<DocumentAccountInfoAgreggate>>()
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

        [HttpGet("Get/filter-search"), MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByID([FromQuery] GetAllAccountRequestModel requestModel, IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            try
            {
                var dto = new AccountDTO(requestModel.AccountKey, int.Parse(requestModel.UserId));
                var resultAsync = await _archiveService.GetByAccountKey(dto);

                return Ok(new StatusCode200TypedResponseModel<IEnumerable<DocumentAccountInfoAgreggate>>()
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

        [HttpDelete("{userId:int}"), MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int userId, IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            try
            {
                var dto = new AccountDTO(0, userId);
                var resultAsync = await _archiveService.InactiveAccount(dto);

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
