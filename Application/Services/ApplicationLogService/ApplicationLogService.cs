using Application.Dtos.ApplicationLog;
using Application.Types;
using Domain.Entitites.ApplicationContextDb;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Web.Api.Toolkit.Entity.Infraestructure.Repositories;
using Web.Api.Toolkit.Helpers.Application.Dtos;

namespace Application.Services.ApplicationLogService
{
    public class ApplicationLogService : IApplicationLogService
    {
        private readonly IBaseRepository<Trace> _traceRepository;
        private readonly IBaseRepository<ApplicationLog> _applicationLogRepository;
        private readonly IBaseRepository<ContextTrace> _contextTraceRepository;
        private readonly ApplicationDbContext _context;

        public ApplicationLogService(
            IBaseRepository<Trace> traceRepository,
            IBaseRepository<ApplicationLog> applicationLogRepository,
            IBaseRepository<ContextTrace> contextTraceRepository,
            ApplicationDbContext context
        )
        {
            _traceRepository = traceRepository;
            _applicationLogRepository = applicationLogRepository;
            _contextTraceRepository = contextTraceRepository;
            _context = context;
        }

        public async Task<int> GetTraceId()
        {
            var trace = new Trace();
            var result = await _traceRepository.AddAsync(trace);

            return result?.Id ?? 0;
        }

        public async Task AddLogAsync(int traceId, string message, string type, string action)
        {
            var log = new ApplicationLog
            {
                TraceId = traceId,
                Message = message,
                Type = type,
                Action = action,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };

            await _applicationLogRepository.AddAsync(log);
        }

        public async Task AddContextTraceAsync<T>(int traceId, string entityName, string entityId)
        {
            var contextTrace = new ContextTrace
            {
                TraceId = traceId,
                EntityName = nameof(T),
                EntityId = entityId,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };

            await _contextTraceRepository.AddAsync(contextTrace);
        }

        public async Task AddContextTraceAsync(int traceId, string entityName, string entityId)
        {
            var contextTrace = new ContextTrace
            {
                TraceId = traceId,
                EntityName = entityName,
                EntityId = entityId,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };

            await _contextTraceRepository.AddAsync(contextTrace);
        }

        public BaseResponse<List<ApplicationLogResponseDto>> GetLogsByTraceId(int traceId)
        {
            var logs = _applicationLogRepository.Get()
                .Where(x => x.TraceId == traceId)
                .OrderBy(x => x.CreateDate)
                .Select(x => new ApplicationLogResponseDto
                {
                    Id = x.Id,
                    Message = x.Message,
                    Type = x.Type,
                    Action = x.Action,
                    TraceId = x.TraceId,
                    CreateDate = x.CreateDate,
                    UpdateDate = x.UpdateDate,
                    UserId = x.UserId
                })
                .ToList();

            var response = new BaseResponse<List<ApplicationLogResponseDto>>(true)
            {
                Data = logs
            };

            return response;
        }

        public BaseResponse<List<TraceResponseDto>> GetAllTraces()
        {
            var traces = _traceRepository.Get()
                .OrderByDescending(x => x.CreateDate)
                .Select(x => new TraceResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    CreateDate = x.CreateDate,
                    UpdateDate = x.UpdateDate,
                    UserId = x.UserId,
                    LogsCount = x.Logs.Count
                })
                .ToList();

            var response = new BaseResponse<List<TraceResponseDto>>(true)
            {
                Data = traces
            };

            return response;
        }

        public BaseResponse<List<ApplicationLogResponseDto>> GetAllLogs()
        {
            var traces = _applicationLogRepository.Get()
                .OrderByDescending(x => x.CreateDate)
                 .Select(x => new ApplicationLogResponseDto
                 {
                     Id = x.Id,
                     Message = x.Message,
                     Type = x.Type,
                     Action = x.Action,
                     TraceId = x.TraceId,
                     CreateDate = x.CreateDate,
                     UpdateDate = x.UpdateDate,
                     UserId = x.UserId
                 })
                .ToList();

            var response = new BaseResponse<List<ApplicationLogResponseDto>>(true)
            {
                Data = traces
            };

            return response;
        }
    }
}

