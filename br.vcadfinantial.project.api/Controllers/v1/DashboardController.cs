using br.vcadfinantial.project.api.Models.Responses;
using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace br.vcadfinantial.project.api.Controllers.v1
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;

        public DashboardController(ILogger<DashboardController> logger, IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        [HttpGet("GetAccount"), MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccount(IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            try
            {
                var resultAsync = await _dashboardService.GetAccount();

                return Ok(new StatusCode200TypedResponseModel<IEnumerable<AccountMinMaxInfoAgreggate>>()
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

        [HttpGet("GetBalance"), MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBalance(IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            try
            {
                var resultAsync = await _dashboardService.GetBalance();

                return Ok(new StatusCode200TypedResponseModel<IEnumerable<AccountBalanceCategoryInfoAgreggate>>()
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
    }
}
