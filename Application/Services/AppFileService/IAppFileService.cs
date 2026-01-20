using Application.Dtos.AppFile;
using Application.Types;
using Domain.Entitites.ApplicationContextDb;
using Domain.Queues.AppFileDtos;
using Web.Api.Toolkit.Helpers.Application.Dtos;

namespace Application.Services.AppFileService
{
    public interface IAppFileService
    {
        Task<BaseResponse<int>> CreateTraceId();
        Task<BaseResponse<bool>> DriverIsConnected();
        BaseResponse<List<AppFileResponseDto>> GetFiles();
        BaseResponse<AppFileResponseDto> GetFileById(int id);
        BaseResponse<List<AppStoredFileResponseDto>> GetAppStoredFiles(int idAppFile);
        Task<BaseResponse<AppFileResponseDto>> InsertFile(AppFileRequestDto req);
        Task<BaseResponse<AppFileResponseDto>> UpdateFile(AppFileRequestDto req, int id);
        Task<BaseResponse<StoredFile>> DownloadFileWithToken(string token);
        Task<DefaultResponse> RequestSync(AppFileSyncRequestDto req);
        Task<DefaultResponse> DeleteFile(int id);
        Task<DefaultResponse> DeleteStoredFile(int id);
        Task<DefaultResponse> SingleSync(AppFileStreamFileRequestDto req);
        Task<DefaultResponse> SetAppFileStatus(int appFileId, AppFileStatusTypes status);
        Task<DefaultResponse> CheckAppFileStatus(CheckAppFileStatusRequestDto request);
        Task<DefaultResponse> DeleteSoftDeletedItems();
    }
}
