using Application.Attributes.Trace;
using Application.Dtos.AppFile;
using Application.Services.AppFileService;
using ASP.NET_Core_Template.Attributes;
using Domain.Queues.AppFileDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Web.Api.Toolkit.Helpers.Api.Extensions;
using Web.Api.Toolkit.Helpers.Application.Dtos;


namespace ASP.NET_Core_Template.Controllers
{
    [ApiController]
    public class DriverController : ControllerBase
    {
        public readonly IAppFileService _appFileService;

        public DriverController
        (
            IAppFileService appFileService
        )
        {
            _appFileService = appFileService;
        }

        [Traced]
        [ApiKeyAuthorize]
        [HttpGet("get-trace-id")]
        public async Task<ActionResult<BaseResponse<int>>> GetTraceId()
        {
            var traceId = await _appFileService.CreateTraceId();
            return this.Result(traceId);
        }

        [Traced]
        [ApiKeyAuthorize]
        [HttpPost("stream-file")]
        public async Task<ActionResult<DefaultResponse>> UploadFile([FromForm] AppFileStreamFileRequestDto req)
        {
            var result = await _appFileService.SingleSync(req);
            return this.DefaultResult(result);
        }
        
        [Traced]
        [ApiKeyAuthorize]
        [HttpPost("request-sync")]
        public async Task<ActionResult<DefaultResponse>> RequestSync([FromBody] AppFileSyncRequestDto req)
        {
            var result = await _appFileService.RequestSync(req);
            return this.DefaultResult(result);
        }

        [Traced(OnCreate = true)]
        [ApiKeyAuthorize]
        [HttpPut("update-appfile-status")]
        public async Task<ActionResult<DefaultResponse>> UpdateAppFileStatus([FromBody] UpdateAppFileStatusRequestDto request)
        {
            var result = await _appFileService.SetAppFileStatus(request.AppFileId, request.Status, request.StatusMessage, request.StatusDetails);
            return this.DefaultResult(result);
        }

        [Traced(OnCreate = true)]
        [ApiKeyAuthorize]
        [HttpPut("update-appstoredfile-status")]
        public async Task<ActionResult<DefaultResponse>> UpdateAppStoredFileStatus([FromBody] UpdateAppStoredFileStatusRequestDto request)
        {
            var result = await _appFileService.SetAppStoredFileStatus(request.AppStoredFileId, request.Status, request.StatusMessage, request.StatusDetails);
            return this.DefaultResult(result);
        }
    }
}
