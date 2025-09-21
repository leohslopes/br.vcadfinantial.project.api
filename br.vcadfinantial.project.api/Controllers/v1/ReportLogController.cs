using br.vcadfinantial.project.api.Models.Requests;
using br.vcadfinantial.project.api.Models.Responses;
using br.vcadfinantial.project.domain.DTO;
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
    public class ReportLogController : ControllerBase
    {
        private readonly ILogger<ReportLogController> _logger;
        private readonly IReportLogService _reportLogService;

        public ReportLogController(ILogger<ReportLogController> logger, IReportLogService reportLogService)
        {
            _logger = logger;
            _reportLogService = reportLogService;
        }

        [HttpGet("filter-report"), MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Download([FromQuery] DownloadReportLogRequestModel requestModel, IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            try
            {
                var dto = new ReportLogDTO(requestModel.MonthKey, requestModel.UserId);
                var resultAsync = await _reportLogService.Export(dto);

                return Ok(new StatusCode200TypedResponseModel<string>()
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
