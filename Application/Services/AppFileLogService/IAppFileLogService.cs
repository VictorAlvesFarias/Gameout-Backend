using Application.Dtos.AppFileLog;
using Application.Types;
using Web.Api.Toolkit.Helpers.Application.Dtos;

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
