using Application.Dtos.AppFileLog;
using Domain.Entitites.Shared;
using Packages.Helpers.Application.Dtos;

namespace Application.Services.AppFileLogService
{
    public interface IAppFileLogService
    {
        Task LogActionAsync(
            AppFileActionType actionType,
            string actionMessage,
            int? appFileId = null,
            int? appStoredFileId = null,
            int? storedFileId = null,
            string? path = null,
            string? recordName = null);

        Task<BaseResponse<List<AppFileLogResponseDto>>> GetLogsAsync(AppFileLogFilterDto filter);
    }
}
