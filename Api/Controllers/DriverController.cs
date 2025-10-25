using Application.Dtos.AppFile;
using Application.Services.AppFileService;
using Domain.Entitites.ApplicationContextDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packages.Helpers.Application.Dtos;
using Packages.Helpers.Api.Extensions;
using Domain.Queues.AppFileDtos;


namespace ASP.NET_Core_Template.Controllers
{
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

        [Authorize]
        [HttpPost("stream-file")]
        public ActionResult<DefaultResponse> UploadFile(int appStoredFileId,int originalFileSize,IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);

            var result = _appFileService.SingleSync(new AppFileUpdateResponseMessag
            {
                AppStoredFileId = appStoredFileId,
                UncompressedSize = originalFileSize,
                MemoryStream = memoryStream.ToArray()
            });

            return this.DefaultResult(result);
        }

        [HttpPost("upload-error")]
        public ActionResult<DefaultResponse> UploadError([FromBody] AppFileErrorMessage req)
        {
            var result = _appFileService.ProcessError(req);
            return this.DefaultResult(result);
        }

        [HttpPost("request-sync")]
        public ActionResult<DefaultResponse> RequestSync([FromBody] AppFileSyncRequestMessage req)
        {
            var result = _appFileService.RequestSync(req.AppFileId);
            return this.DefaultResult(result);
        }

        [HttpPost("status")]
        public ActionResult<DefaultResponse> Status([FromBody] AppFileStatusCheckResponseMessage req)
        {
            var result = _appFileService.ProcessStatusResponse(req);
            return this.DefaultResult(result);
        }

        [HttpPost("validate")]
        public ActionResult<DefaultResponse> Validate([FromBody] AppFileValidateStatusResponse req)
        {
            var result = _appFileService.StatusUpdate(req);
            return this.DefaultResult(result);
        }
    }
}
