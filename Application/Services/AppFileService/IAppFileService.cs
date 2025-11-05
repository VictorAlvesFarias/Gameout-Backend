using Application.Dtos.AppFile;
using Domain.Entitites.ApplicationContextDb;
using Domain.Queues.AppFileDtos;
using Web.Api.Toolkit.Helpers.Application.Dtos;

namespace Application.Services.AppFileService
{
    public interface IAppFileService
    {
        BaseResponse<List<AppFileResponseDto>> GetFiles();
        BaseResponse<List<AppStoredFileResponseDto>> GetAppStoredFiles(int? idAppFile = null, bool? processing = false);
        BaseResponse<AppFile> InsertFile(AppFile req);
        BaseResponse<AppFile> UpdateFile(AppFile req, int id);
        BaseResponse<StoredFile> DownloadFile(int id);
        DefaultResponse RequestSync(int idAppFile);
        DefaultResponse ReprocessFile(int appStoredFileId);
        DefaultResponse CheckProcessingStatus(int appStoredFileId);
        DefaultResponse DeleteFileWithError(int appStoredFileId);
        DefaultResponse DeleteFile(int id);
        DefaultResponse DeleteStoredFile(int id);
        DefaultResponse RequestStatusUpdate(int appStoredFileId);
        DefaultResponse SingleSync(AppFileUpdateResponseMessag req);
        DefaultResponse ProcessError(AppFileErrorMessage errorMessage);
        DefaultResponse ProcessStatusResponse(AppFileStatusCheckResponseMessage statusResponse);
        DefaultResponse StatusUpdate(AppFileValidateStatusResponse req);
    }
}
