using Application.Dtos.ApplicationLog;
using Application.Types;
using Web.Api.Toolkit.Helpers.Application.Dtos;

namespace Application.Services.ApplicationLogService
{
    public interface IApplicationLogService
    {
        Task<int> GetTraceId();
        Task AddLogAsync(int traceId, string message, string type, string action);
        Task AddContextTraceAsync(int traceId, string entityName, string entityId);
        Task AddContextTraceAsync<T>(int traceId, string entityName, string entityId);
        BaseResponse<List<ApplicationLogResponseDto>> GetLogsByTraceId(int traceId);
        BaseResponse<List<TraceResponseDto>> GetAllTraces();
        BaseResponse<List<ApplicationLogResponseDto>> GetAllLogs();
    }
}

