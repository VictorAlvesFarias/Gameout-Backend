using Application.Attributes.Trace;
using Application.Dtos.AppFile;
using Application.Services.AppFileService;
using Domain.Queues.AppFileDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Web.Api.Toolkit.Helpers.Api.Extensions;
using Web.Api.Toolkit.Helpers.Application.Dtos;


namespace ASP.NET_Core_Template.Controllers
{
    [ApiController]
    public class AppFileDriverController : ControllerBase
    {
        public readonly IAppFileService _appFileService;

        public AppFileDriverController
        (
            IAppFileService appFileService
        )
        {
            _appFileService = appFileService;
        }

        [Traced]
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        [HttpGet("get-trace-id")]
        public async Task<ActionResult<BaseResponse<int>>> GetTraceId()
        {
            var traceId = await _appFileService.CreateTraceId();
            return this.Result(traceId);
        }

        [Traced]
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        [HttpPost("stream-file")]
        public async Task<ActionResult<DefaultResponse>> UploadFile([FromForm] AppFileStreamFileRequestDto req)
        {
            var result = await _appFileService.SingleSync(req);
            return this.DefaultResult(result);
        }
        
        [Traced]
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        [HttpPost("request-sync")]
        public async Task<ActionResult<DefaultResponse>> RequestSync([FromBody] AppFileSyncRequestDto req)
        {
            var result = await _appFileService.RequestSync(req);
            return this.DefaultResult(result);
        }

        [Traced(OnCreate = true)]
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        [HttpPut("update-appfile-status")]
        public async Task<ActionResult<DefaultResponse>> UpdateAppFileStatus([FromBody] UpdateAppFileStatusRequestDto request)
        {
            var result = await _appFileService.SetAppFileStatus(request.AppFileId, request.Status);
            return this.DefaultResult(result);
        }

        [Traced(OnCreate = true)]
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        [HttpPut("update-appstoredfile-status")]
        public async Task<ActionResult<DefaultResponse>> UpdateAppStoredFileStatus([FromBody] UpdateAppStoredFileStatusRequestDto request)
        {
            var result = await _appFileService.SetAppStoredFileStatus(request.AppStoredFileId, request.Status);
            return this.DefaultResult(result);
        }
    }
}
