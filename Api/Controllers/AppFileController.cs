using Application.Dtos.AppFile;
using Application.Services.AppFileService;
using Domain.Entitites.ApplicationContextDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Packages.Helpers.Application.Dtos;
using Packages.Helpers.Api.Extensions;

namespace ASP.NET_Core_Template.Controllers
{
    public class AppFileController : Controller
    {

        public readonly IAppFileService _appFileService;
        private readonly Controller _controller;

        public AppFileController
        (
            IAppFileService appFileService
        )
        {
            _appFileService = appFileService;
            _controller = this;
        }

        [Authorize]
        [HttpPost("upload-file")]
        public ActionResult<BaseResponse<AppFile>> UploadFile([FromBody] AppFile file)
        {
            var result = _appFileService.InsertFile(file);
            return this.Result(result);
        }

        [Authorize]
        [HttpPut("update-file")]
        public ActionResult<BaseResponse<AppFile>> UpdateFile([FromBody] AppFile file, int id)
        {
            var result = _appFileService.UpdateFile(file, id);
            return this.Result(result);
        }

        [Authorize]
        [HttpDelete("delete-file")]
        public ActionResult<DefaultResponse> DeleteFile(int id)
        {
            var result = _appFileService.DeleteFile(id);
            return this.DefaultResult(result);
        }

        [Authorize]
        [HttpDelete("delete-stored-file")]
        public ActionResult<DefaultResponse> DeleteStoredFile(int id)
        {
            var result = _appFileService.DeleteStoredFile(id);
            return this.DefaultResult(result);
        }

        [Authorize]
        [HttpGet("get-files")]
        public ActionResult<BaseResponse<List<AppFileResponseDto>>> GetFiles()
        {
            var result = _appFileService.GetFiles();
            return this.Result(result);
        }

        [Authorize]
        [HttpGet("get-stored-files")]
        public ActionResult<BaseResponse<List<AppStoredFileResponseDto>>> GetStoredFiles(int? idAppFile = null, bool? processing = false)
        {
            var result = _appFileService.GetAppStoredFiles(idAppFile, processing);
            return this.Result(result);
        }

        [Authorize]
        [HttpPost("single-sync")]
        public ActionResult<DefaultResponse> SingleSync(int idAppFile)
        {
            var result = _appFileService.RequestSync(idAppFile);
            return this.DefaultResult(result);
        }

        [Authorize]
        [HttpPost("reprocess-file")]
        public ActionResult<DefaultResponse> ReprocessFile(int appStoredFileId)
        {
            var result = _appFileService.ReprocessFile(appStoredFileId);
            return this.DefaultResult(result);
        }

        [Authorize]
        [HttpDelete("delete-file-with-error")]
        public ActionResult<DefaultResponse> DeleteFileWithError(int appStoredFileId)
        {
            var result = _appFileService.DeleteFileWithError(appStoredFileId);
            return this.DefaultResult(result);
        }

        [Authorize]
        [HttpGet("check-processing-status")]
        public ActionResult<DefaultResponse> CheckProcessingStatus(int appStoredFileId)
        {
            var result = _appFileService.CheckProcessingStatus(appStoredFileId);
            return this.DefaultResult(result);
        }

        [Authorize]
        [HttpGet("validate-status")]
        public ActionResult<DefaultResponse> ValidateStatus(int appFileId)
        {
            var result = _appFileService.RequestStatusUpdate(appFileId);
            return this.DefaultResult(result);
        }

        [Authorize]
        [HttpGet("download-file")]
        public IActionResult DownloadFile(int id)
        {
            var result = _appFileService.DownloadFile(id);
            return this.FileResult(result);
        }
    }
}
