using Application.Dtos.AppFileLog;
using Domain.Entitites.ApplicationContextDb;
using Domain.Entitites.Shared;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Packages.Entity.Infraestructure.Repositories;
using Packages.Helpers.Application.Dtos;

namespace Application.Services.AppFileLogService
{
    public class AppFileLogService : IAppFileLogService
    {
        private readonly IBaseRepository<AppFileLog> _appFileLogRepository;
        private readonly ApplicationDbContext _context;
        private readonly IBaseRepository<AppFile> _appFileRepository;

        public AppFileLogService(
            IBaseRepository<AppFileLog> appFileLogRepository,
            ApplicationDbContext context,
            IBaseRepository<AppFile> appFileRepository)
        {
            _appFileLogRepository = appFileLogRepository;
            _context = context;
            _appFileRepository = appFileRepository;
        }

        public async Task LogActionAsync(
            AppFileActionType actionType,
            string actionMessage,
            int? appFileId = null,
            int? appStoredFileId = null,
            int? storedFileId = null,
            string? path = null,
            string? recordName = null)
        {
            try
            {
                var log = new AppFileLog
                {
                    AppFileId = appFileId,
                    AppStoredFileId = appStoredFileId,
                    StoredFileId = storedFileId,
                    Path = path ?? string.Empty,
                    RecordName = recordName ?? string.Empty,
                    ActionMessage = actionMessage,
                    ActionType = (int)actionType,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };

                // Definir o usuário atual se disponível
                var userId = _context.GetUserId();
                if (!string.IsNullOrEmpty(userId))
                {
                    log.UserId = userId;
                }

                await _appFileLogRepository.AddAsync(log);
            }
            catch (Exception ex)
            {
                // Log error to prevent breaking the main application
                // In production, consider using an appropriate logger
                Console.WriteLine($"Error logging action: {ex.Message}");
            }
        }

        public async Task<BaseResponse<List<AppFileLogResponseDto>>> GetLogsAsync(AppFileLogFilterDto filter)
        {
            try
            {
                var query = _appFileLogRepository.Get().AsQueryable();

                // Aplicar filtros
                if (filter.AppFileId.HasValue)
                    query = query.Where(x => x.AppFileId == filter.AppFileId.Value);

                if (filter.AppStoredFileId.HasValue)
                    query = query.Where(x => x.AppStoredFileId == filter.AppStoredFileId.Value);

                if (filter.StoredFileId.HasValue)
                    query = query.Where(x => x.StoredFileId == filter.StoredFileId.Value);

                if (filter.ActionType.HasValue)
                    query = query.Where(x => x.ActionType == (int)filter.ActionType.Value);

                if (filter.StartDate.HasValue)
                    query = query.Where(x => x.CreateDate >= filter.StartDate.Value);

                if (filter.EndDate.HasValue)
                    query = query.Where(x => x.CreateDate <= filter.EndDate.Value);

                if (!string.IsNullOrEmpty(filter.UserId))
                    query = query.Where(x => x.UserId == filter.UserId);

                // Ordenar por data de criação (mais recentes primeiro)
                query = query.OrderByDescending(x => x.CreateDate);

                // Aplicar paginação
                var totalCount = await query.CountAsync();
                var logs = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(x => new AppFileLogResponseDto
                    {
                        Id = x.Id,
                        AppFileId = x.AppFileId,
                        AppStoredFileId = x.AppStoredFileId,
                        StoredFileId = x.StoredFileId,
                        Path = x.Path,
                        RecordName = x.RecordName,
                        ActionMessage = x.ActionMessage,
                        ActionType = x.ActionType,
                        CreateDate = x.CreateDate,
                        UpdateDate = x.UpdateDate,
                        UserId = x.UserId
                    })
                    .ToListAsync();

                var response = new BaseResponse<List<AppFileLogResponseDto>>(true)
                {
                    Data = logs
                };

                return response;
            }
            catch (Exception ex)
            {
                var response = new BaseResponse<List<AppFileLogResponseDto>>(false);
                response.AddError(new ErrorMessage($"Error retrieving logs: {ex.Message}"));
                return response;
            }
        }
    }
}
