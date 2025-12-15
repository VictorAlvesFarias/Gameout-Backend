using Application.Attributes.Trace;
using Application.Dtos.ApplicationLog;
using Application.Services.ApplicationLogService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Toolkit.Helpers.Api.Extensions;
using Web.Api.Toolkit.Helpers.Application.Dtos;

namespace ASP.NET_Core_Template.Controllers
{
    [ApiController]
    [Route("api/application-log")]
    [Authorize]
    public class ApplicationLogController : ControllerBase
    {
        private readonly IApplicationLogService _applicationLogService;

        public ApplicationLogController(IApplicationLogService applicationLogService)
        {
            _applicationLogService = applicationLogService;
        }

        [HttpGet("traces")]
        public ActionResult<BaseResponse<List<ApplicationLogResponseDto>>> GetAllLogs()
        {
            var result = _applicationLogService.GetAllLogs();
            return this.Result(result);
        }

        [Traced]
        [HttpPost("add")]
        public async Task<ActionResult<BaseResponse<ApplicationLogResponseDto>>> AddLog([FromBody] ApplicationLogRequestDto request)
        {
            var result = await _applicationLogService.AddLog(request);
            return this.Result(result);
        }
    }
}

