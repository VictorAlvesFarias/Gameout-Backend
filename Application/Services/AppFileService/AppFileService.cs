using Application.Dtos.AppFile;
using Application.Extensions;
using Application.Services.ApplicationLogService;
using Application.Types;
using Application.Workers;
using Domain.Entitites.ApplicationContextDb;
using Domain.Queues.AppFileDtos;
using Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO.Compression;
using Web.Api.Toolkit.Entity.Infraestructure.Repositories;
using Web.Api.Toolkit.Helpers.Application.Dtos;
using Web.Api.Toolkit.Ws.Application.Dtos;

namespace Application.Services.AppFileService
{
    public class AppFileService : IAppFileService
    {
        private readonly IBaseRepository<AppFile> _appFileRepository;
        private readonly IBaseRepository<AppStoredFile> _appStoredFileRepository;
        private readonly IBaseRepository<StoredFile> _storedFileRepository;
        private readonly ApplicationContext _applicationContext;
        private readonly ApplicationDbContext _dbContext;
        private readonly AppFileWorker _webSocketWorker;
        private readonly IApplicationLogService _applicationLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppFileService(
            IBaseRepository<AppFile> appFileRepository,
            IBaseRepository<StoredFile> storedFileRepository,
            IBaseRepository<AppStoredFile> appStoredFileRepository,
            ApplicationContext applicationContext,
            ApplicationDbContext dbContext,
            AppFileWorker webSocketWorker,
            IApplicationLogService applicationLogService,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _appFileRepository = appFileRepository;
            _storedFileRepository = storedFileRepository;
            _appStoredFileRepository = appStoredFileRepository;
            _applicationContext = applicationContext;
            _dbContext = dbContext;
            _webSocketWorker = webSocketWorker;
            _applicationLogService = applicationLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseResponse<int>> CreateTraceId()
        {
            var traceId = await _applicationLogService.GetTraceId();
            var response = new BaseResponse<int>(true)
            {
                Data = traceId
            };
            return response;
        }

        public async Task<BaseResponse<AppFileResponseDto>> InsertFile(AppFileRequestDto req)
        {
            var traceId = await _applicationLogService.GetTraceId();
            var appFile = new AppFile
            {
                Name = req.Name,
                Path = req.Path,
                VersionControl = req.VersionControl,
                Observer = req.Observer,
                AutoValidateSync = req.AutoValidateSync,
                Status = (int)AppFileStatusTypes.Unsynced
            };
            var appFileAddResult = await _appFileRepository.AddAsync(appFile);
            var response = new BaseResponse<AppFileResponseDto>(appFileAddResult is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to insert file. Please check the provided data and try again."));

                await _applicationLogService.AddLogAsync(traceId, "File inserted unsuccessfully", ApplicationLogType.Message, ApplicationLogAction.Error);
            }
            else
            {
                await _applicationLogService.AddLogAsync(traceId, "File inserted successfully", ApplicationLogType.Message, ApplicationLogAction.Success);

                response.Data = new AppFileResponseDto
                {
                    Id = appFile.Id,
                    Name = appFile.Name,
                    Path = appFile.Path,
                    VersionControl = appFile.VersionControl,
                    Observer = appFile.Observer,
                    CreateDate = appFile.CreateDate,
                    UpdateDate = appFile.UpdateDate,
                    UserId = appFile.UserId,
                    AutoValidateSync = appFile.AutoValidateSync,
                    Status = appFile.Status,
                    StatusDetails = appFile.StatusDetails,
                    StatusMessage = appFile.StatusMessage
                };
            }

            return response;
        }

        public BaseResponse<List<AppFileResponseDto>> GetFiles()
        {
            var result = _appFileRepository.Get().OrderByDescending(e => e.Id).ToList();
            var response = new BaseResponse<List<AppFileResponseDto>>();
            var responseMapper = (AppFile e) => new AppFileResponseDto()
            {
                Id = e.Id,
                Name = e.Name,
                Path = e.Path,
                VersionControl = e.VersionControl,
                Observer = e.Observer,
                CreateDate = e.CreateDate,
                UpdateDate = e.UpdateDate,
                AutoValidateSync = e.AutoValidateSync,
                Status = e.Status,
                StatusDetails = e.StatusDetails,
                StatusMessage = e.StatusMessage
            };
            var traceId = _applicationLogService.GetTraceId().Result;

            response.Data = result.Select(responseMapper).ToList();

            return response;
        }

        public async Task<BaseResponse<StoredFile>> DownloadFileWithToken(string token)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            if (httpContext == null)
            {
                var errorResponse = new BaseResponse<StoredFile>();
                errorResponse.AddError(new ErrorMessage("HTTP context not available", 500));
                return errorResponse;
            }

            // O token já foi validado pelo ValidateDownloadTokenAttribute
            // Apenas pegamos o fileId do HttpContext.Items
            if (!httpContext.Items.TryGetValue("DownloadAuth_FileId", out var fileIdObj) || 
                !int.TryParse(fileIdObj?.ToString(), out var fileId))
            {
                var errorResponse = new BaseResponse<StoredFile>();
                errorResponse.AddError(new ErrorMessage("File ID not found in token", 400));
                return errorResponse;
            }

            // O mediator já vai filtrar pelo userId que foi colocado no HttpContext.User pelo attribute
            var appStoredFile = _appStoredFileRepository.Get().FirstOrDefault(e => e.Id == fileId);
            
            if (appStoredFile == null)
            {
                var errorResponse = new BaseResponse<StoredFile>();
                errorResponse.AddError(new ErrorMessage("File not found or you don't have permission to download it", 404));
                return errorResponse;
            }

            var result = _storedFileRepository.Get().FirstOrDefault(e => e.Id == appStoredFile.StoredFileId);
            var response = new BaseResponse<StoredFile>(result is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to download file. The file may not exist or may be corrupted."));
            }
            else
            {
                // Garantir que o nome do arquivo tenha a extensão .zip
                if (!string.IsNullOrEmpty(result.Name) && !result.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    result.Name = $"{result.Name}.zip";
                }

                response.Data = result;
            }

            return response;
        }

        public async Task<BaseResponse<AppFileResponseDto>> UpdateFile(AppFileRequestDto req, int id)
        {
            var traceId = await _applicationLogService.GetTraceId();
            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == id);
            var response = new BaseResponse<AppFileResponseDto>(appFile is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to update file. The file with the specified ID was not found."));

                await _applicationLogService.AddLogAsync(traceId, "File update failed because file was not found", ApplicationLogType.Message, ApplicationLogAction.Error);
                

                return response;
            }

            appFile.Update(req.Name, req.Path, req.VersionControl, req.Observer, req.AutoValidateSync);

            response.Success = _appFileRepository.Update(appFile);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to update file. Database operation was unsuccessful."));

                await _applicationLogService.AddLogAsync(traceId, "File update failed because database operation was unsuccessful", ApplicationLogType.Message, ApplicationLogAction.Error);

                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, "File updated successfully", ApplicationLogType.Message, ApplicationLogAction.Success);

            var clientDriver = _webSocketWorker.GetDriveClient(appFile.UserId);

            if (clientDriver is not null)
            {
                var headers = new Dictionary<string, string>();

                headers.Add("X-Trace-Application-Id", traceId.ToString());

                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "SetEvents",
                    Body = "",
                    Headers = headers
                });
            }
            else
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The driver is not connected."));
               
                await _applicationLogService.AddLogAsync(traceId, "Sync request failed because driver is not connected", ApplicationLogType.Message, ApplicationLogAction.Error);

                return response;
            }

            response.Data = new AppFileResponseDto
            {
                Id = appFile.Id,
                Name = appFile.Name,
                Path = appFile.Path,
                VersionControl = appFile.VersionControl,
                Observer = appFile.Observer,
                CreateDate = appFile.CreateDate,
                UpdateDate = appFile.UpdateDate,
                UserId = appFile.UserId,
                AutoValidateSync = appFile.AutoValidateSync,
                Status = appFile.Status,
                StatusDetails = appFile.StatusDetails,
                StatusMessage = appFile.StatusMessage
            };

            return response;
        }

        public BaseResponse<List<AppStoredFileResponseDto>> GetAppStoredFiles(int? idAppFile = null, bool? processing = false)
        {
            var traceId = _applicationLogService.GetTraceId().Result;

            var appStoredFilesQuery = _appStoredFileRepository
                .Get()
                .Include(e => e.StoredFile)
                .Include(e => e.AppFile)
                .Where(e =>
                    idAppFile != null? e.AppFileId == idAppFile : true
                );

            IQueryable<AppStoredFile> finalQuery;

            if (!(processing ?? false))
            {
                finalQuery = appStoredFilesQuery
                    .Where(e =>
                        e.StoredFileId != null &&
                        (
                            e.Status == (int)AppStoredFileStatusTypes.Complete
                        )
                    )
                    .Select(e => new AppStoredFile()
                    {
                        StoredFile = new StoredFile
                        {
                            Id = e.StoredFile.Id,
                            Name = e.StoredFile.Name,
                            MimeType = e.StoredFile.MimeType,
                            CreateDate = e.StoredFile.CreateDate,
                            UpdateDate = e.StoredFile.UpdateDate,
                            SizeInBytes = e.StoredFile.SizeInBytes,
                            Bytes = null
                        },
                        AppFileId = e.AppFileId,
                        CreateDate = e.CreateDate,
                        UpdateDate = e.UpdateDate,
                        Status = e.Status,
                        StatusDetails = e.StatusDetails,
                        StatusMessage = e.StatusMessage,
                        Id = e.Id,
                    });                                             
            }
            else
            {
                finalQuery = appStoredFilesQuery
                    .Where(e =>
                        e.StoredFileId == null &&
                        (
                            e.Status == (int)AppStoredFileStatusTypes.Processing ||
                            e.Status == (int)AppStoredFileStatusTypes.Error ||
                            e.Status == (int)AppStoredFileStatusTypes.PathNotFounded ||
                            e.Status == (int)AppStoredFileStatusTypes.LockedFiles ||
                            e.Status == null
                        )
                    );
            }

            var appStoredFiles = finalQuery.ToList();
            var response = new BaseResponse<List<AppStoredFileResponseDto>>(appStoredFiles is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to retrieve stored files. Please try again later."));

                _applicationLogService.AddLogAsync(traceId, "Failed to retrieve stored files. Please try again later.", ApplicationLogType.Message, ApplicationLogAction.Error).Wait();
            }
            else
            {
                response.Data = appStoredFiles.Select(e =>
                    new AppStoredFileResponseDto()
                    {
                        Id = e.Id,
                        AppFileId = e.AppFileId,
                        CreateDate = e.CreateDate,
                        UpdateDate = e.UpdateDate,
                        StoredFileId = e.StoredFileId,
                        Versioned = e.Versioned,
                        Name = e?.AppFile?.Name,
                        Path = e?.AppFile?.Path,
                        SizeInBytes = e?.StoredFile?.SizeInBytes,
                        Status = e.Status,
                        StatusDetails = e.StatusDetails,
                        StatusMessage = e.StatusMessage
                    }
                ).ToList();
            }

            return response;
        }

        public async Task<DefaultResponse> RequestSync(AppFileSyncRequestDto req)
        {
            var traceId = await _applicationLogService.GetTraceId();
            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == req.IdAppFile);
            var response = new DefaultResponse(appFile != null);

            if (!response.Success)
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The file with the specified ID was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Sync request failed because file was not found", ApplicationLogType.Message, ApplicationLogAction.Error);

                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, "Sync request initiated", ApplicationLogType.Message, ApplicationLogAction.Info, $"Entity: AppFile, ID: {req.IdAppFile}, Path: {appFile.Path}");

            var appStoredFile = new AppStoredFile
            {
                AppFileId = req.IdAppFile,
                Versioned = false,
                Status = (int)AppStoredFileStatusTypes.Processing,
                StatusMessage = "Processing"
            };
            var appStoredFileAddResult = _appStoredFileRepository.AddAsync(appStoredFile).Result;
            
            if (appStoredFileAddResult is null)
            {
                response.Errors.Add(new ErrorMessage("Failed to create stored file record. Database operation was unsuccessful."));

                await _applicationLogService.AddLogAsync(traceId, "Failed to create stored file record", ApplicationLogType.Message, ApplicationLogAction.Error, $"Entity: AppFile, ID: {req.IdAppFile}, Path: {appFile.Path}");
               
                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, "AppStoredFile created successfully", ApplicationLogType.Message, ApplicationLogAction.Success, $"Entity: AppStoredFile, ID: {appStoredFileAddResult.Id}, AppFileId: {req.IdAppFile}, Path: {appFile.Path}, Status: Processing");

            this.SetAppFileStatus(req.IdAppFile, AppFileStatusTypes.Pending);

            var clientDriver = _webSocketWorker.GetDriveClient(appFile.UserId);

            if (clientDriver is not null)
            {
                var headers = new Dictionary<string, string>();

                headers.Add("X-Trace-Application-Id", traceId.ToString());

                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "SingleSync",
                    Body = new AppFileUpdateRequestMessage()
                    {
                        AppStoredFileId = appStoredFileAddResult.Id,
                        AppFileId = appFile.Id,
                        Path = appFile.Path,
                    },
                    Headers = headers
                });
            }
            else
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The driver is not connected."));
                
                await _applicationLogService.AddLogAsync(traceId, "Sync request failed because driver is not connected", ApplicationLogType.Message, ApplicationLogAction.Error);

                return response;
            }

            var client = _webSocketWorker.GetWebClient(appFile.UserId);

            if (client is not null)
            {
                _webSocketWorker.SendAsync(client.Id, new WebSocketRequest()
                {
                    Body = null,
                    Event = "NewsFilesRequestPing"
                });
            }

            await _applicationLogService.AddLogAsync(traceId, "Sync request sent to driver successfully", ApplicationLogType.Message, ApplicationLogAction.Success);

            return response;
        }

        public async Task<DefaultResponse> SingleSync(AppFileStreamFileRequestDto req)
        {
            var traceId = await _applicationLogService.GetTraceId();
            var response = new DefaultResponse();
            var appStoredFile = _appStoredFileRepository.Get().Include(e => e.AppFile).FirstOrDefault(e => e.Id == req.AppStoredFileId);

            if (appStoredFile is null)
            {
                response.AddError(new ErrorMessage("Failed to perform single sync. The app stored file record was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Single sync failed because AppStoredFile was not found", ApplicationLogType.Message, ApplicationLogAction.Error);

                return response;
            }

            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == appStoredFile.AppFileId);

            if (appFile is null)
            {
                response.AddError(new ErrorMessage("Failed to perform single sync. The app file record was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Single sync failed because AppFile was not found", ApplicationLogType.Message, ApplicationLogAction.Error);

                return response;
            }

            using var memoryStream = new MemoryStream();

            await req.File.CopyToAsync(memoryStream);
            
            var fileBytes = memoryStream.ToArray();

            if (fileBytes.Length == 0)
            {
                response.AddError(new ErrorMessage("Failed to perform single sync. The uploaded file is empty."));
                await _applicationLogService.AddLogAsync(traceId, "Single sync failed because uploaded file is empty", ApplicationLogType.Message, ApplicationLogAction.Error);
                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, $"File received with {fileBytes.Length} bytes", ApplicationLogType.Message, ApplicationLogAction.Info);

            var appStoredFileOld = _appStoredFileRepository.Get()
                .Where(e => e.Status == (int)AppStoredFileStatusTypes.Complete && e.AppFileId == appFile.Id)
                .Include(e => e.AppFile)
                .OrderByDescending(e => e.CreateDate)
                .FirstOrDefault();

            var storedFile = new StoredFile()
            {
                Bytes = fileBytes,
                Name = appStoredFile.AppFile.Name,
                MimeType = "application/zip",
                SizeInBytes = req.OriginalFileSize
            };
            var addedStoredFile = _storedFileRepository.AddAsync(storedFile).Result;

            if (appStoredFile is null)
            {
                response.AddError(new ErrorMessage("Failed to perform single sync. The stored file record was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Single sync failed because AppStoredFile was not found", ApplicationLogType.Message, ApplicationLogAction.Error);

                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, "Stream received from driver successfully", ApplicationLogType.Message, ApplicationLogAction.Success);

            appStoredFile.Update(storedFileId: addedStoredFile.Id, status: (int)AppStoredFileStatusTypes.Complete, statusMessage: "Complete");

            var appStoredFileUpdatedResult = _appStoredFileRepository.Update(appStoredFile);

            if (!appStoredFileUpdatedResult)
            {
                response.AddError(new ErrorMessage("Failed to update stored file record. Database operation was unsuccessful."));

                return response;
            }

            // Verificar status de todos os AppStoredFiles para atualizar AppFile
            var allAppStoredFiles = _appStoredFileRepository.Get()
                .Where(e => e.AppFileId == appFile.Id)
                .ToList();

            var hasProcessing = allAppStoredFiles.Any(e => 
                e.Status == (int)AppStoredFileStatusTypes.Processing || 
                e.Status == null);
            var hasPathNotFounded = allAppStoredFiles.Any(e => e.Status == (int)AppStoredFileStatusTypes.PathNotFounded);
            var hasLockedFiles = allAppStoredFiles.Any(e => e.Status == (int)AppStoredFileStatusTypes.LockedFiles);
            var hasErrors = allAppStoredFiles.Any(e => e.Status == (int)AppStoredFileStatusTypes.Error);
            var allComplete = allAppStoredFiles.All(e => 
                e.Status == (int)AppStoredFileStatusTypes.Complete);

            if (hasProcessing)
            {
                // Ainda tem arquivos sendo processados
                appFile.Update(status: (int)AppFileStatusTypes.Processing, statusMessage: AppFileStatusTypes.Processing.GetDescription(), statusDetails: ((int)AppFileStatusTypes.Processing).ToString());
            }
            else if (allComplete)
            {
                // Todos processados com sucesso
                appFile.Update(status: (int)AppFileStatusTypes.Synced, statusMessage: AppFileStatusTypes.Synced.GetDescription(), statusDetails: ((int)AppFileStatusTypes.Synced).ToString());
            }
            else if (hasPathNotFounded)
            {
                // Tem arquivos com caminho não encontrado
                appFile.Update(status: (int)AppFileStatusTypes.PathNotFounded, statusMessage: AppFileStatusTypes.PathNotFounded.GetDescription(), statusDetails: ((int)AppFileStatusTypes.PathNotFounded).ToString());
            }
            else if (hasErrors)
            {
                // Tem arquivos com erro genérico ou outros erros
                appFile.Update(status: (int)AppFileStatusTypes.Unsynced, statusMessage: AppFileStatusTypes.Unsynced.GetDescription(), statusDetails: ((int)AppFileStatusTypes.Unsynced).ToString());
            }
            
            var appFileUpdatedResult = _appFileRepository.Update(appFile);

            if (!appFileUpdatedResult)
            {
                response.AddError(new ErrorMessage("Failed to update app file status. Database operation was unsuccessful."));

                return response;
            }

            if (appFile.VersionControl && appStoredFileOld is not null)
            {
                appStoredFileOld.Update(versioned: true);

                var odlAppStoredFileUpdatedResult = _appStoredFileRepository.Update(appStoredFileOld);

                if (!odlAppStoredFileUpdatedResult)
                {
                    response.AddError(new ErrorMessage("Failed to update app stored file record. Database operation was unsuccessful."));

                    return response;
                }
            }
            else if(appStoredFileOld is not null)
            {
                // Verifica se o appStoredFileOld tem um StoredFile relacionado
                var hasStoredFile = appStoredFileOld.StoredFileId.HasValue;
                
                if (hasStoredFile)
                {
                    // Soft delete - marca como deletado
                    appStoredFileOld.IsDeleted = true;
                    appStoredFileOld.DeletedAt = DateTime.Now;
                    
                    var odlAppStoredFileUpdatedResult = _appStoredFileRepository.Update(appStoredFileOld);

                    if (!odlAppStoredFileUpdatedResult)
                    {
                        response.AddError(new ErrorMessage("Failed to soft delete old app stored file record. Database operation was unsuccessful."));
                        return response;
                    }
                }
                else
                {
                    // Hard delete - remove completamente
                    var odlAppStoredFileUpdatedResult = _appStoredFileRepository.Remove(appStoredFileOld);

                    if (!odlAppStoredFileUpdatedResult)
                    {
                        response.AddError(new ErrorMessage("Failed to remove app stored file record. Database operation was unsuccessful."));
                        return response;
                    }
                }
            }

            var client = _webSocketWorker.GetWebClient(appFile.UserId);

            if (client is not null)
            {
                _webSocketWorker.SendAsync(client.Id, new WebSocketRequest()
                {
                    Body = null,
                    Event = "AppFileUpdatedPing"
                });
            }

            await _applicationLogService.AddLogAsync(traceId, "Processing completed successfully", ApplicationLogType.Message, ApplicationLogAction.Success);

            return response;
        }

        public async Task<DefaultResponse> DeleteFile(int id)
        {
            var traceId = await _applicationLogService.GetTraceId();
            var appFile = _appFileRepository.Get()
                .Include(e => e.User)
                .FirstOrDefault(e => e.Id == id);
            var response = new DefaultResponse(appFile is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage($"Falha ao excluir o arquivo. O arquivo com o ID {id} não foi encontrado."));
                await _applicationLogService.AddLogAsync(traceId, "File deletion failed because file was not found", ApplicationLogType.Message, ApplicationLogAction.Error);
                return response;
            }

            // Verifica se existem AppStoredFiles relacionados (incluindo os deletados)
            var hasRelatedAppStoredFiles = _dbContext.AppStoredFile
                .IgnoreQueryFilters()
                .Any(e => e.AppFileId == id);

            if (hasRelatedAppStoredFiles)
            {
                // Soft delete - marca como deletado mas não remove do banco
                appFile.IsDeleted = true;
                appFile.DeletedAt = DateTime.Now;
                var updateResult = _appFileRepository.Update(appFile);
                response.Success = updateResult;

                if (!response.Success)
                {
                    response.AddError(new ErrorMessage("Falha ao excluir o arquivo (soft delete). A operação no banco de dados não foi bem-sucedida."));
                    await _applicationLogService.AddLogAsync(traceId, "File soft deletion failed because database operation was unsuccessful", ApplicationLogType.Message, ApplicationLogAction.Error);
                }
                else
                {
                    await _applicationLogService.AddLogAsync(traceId, "File soft deleted successfully", ApplicationLogType.Message, ApplicationLogAction.Success);
                }
            }
            else
            {
                // Hard delete - remove completamente do banco
                var removeAppFileResult = _appFileRepository.Remove(appFile);
                response.Success = removeAppFileResult;

                if (!response.Success)
                {
                    response.AddError(new ErrorMessage("Falha ao excluir o arquivo. A operação no banco de dados não foi bem-sucedida."));
                    await _applicationLogService.AddLogAsync(traceId, "File deletion failed because database operation was unsuccessful", ApplicationLogType.Message, ApplicationLogAction.Error);
                }
                else
                {
                    await _applicationLogService.AddLogAsync(traceId, "File deleted successfully", ApplicationLogType.Message, ApplicationLogAction.Success);
                }
            }

            return response;
        }

        public async Task<DefaultResponse> DeleteStoredFile(int id)
        {
            var traceId = await _applicationLogService.GetTraceId();
            var appStoredFile = _appStoredFileRepository.Get()
                .Include(e => e.StoredFile)
                .FirstOrDefault(e => e.Id == id);
            var response = new DefaultResponse(appStoredFile is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage($"Failed to delete stored file. The record with ID {id} was not found."));
                await _applicationLogService.AddLogAsync(traceId, "Stored file deletion failed because record was not found", ApplicationLogType.Message, ApplicationLogAction.Error);
                return response;
            }

            // Verifica se existe um StoredFile relacionado
            if (appStoredFile.StoredFileId.HasValue && appStoredFile.StoredFile != null)
            {
                // Soft delete - marca como deletado mas não remove do banco
                appStoredFile.IsDeleted = true;
                appStoredFile.DeletedAt = DateTime.Now;
                
                // Também marca o StoredFile como deletado
                appStoredFile.StoredFile.IsDeleted = true;
                appStoredFile.StoredFile.DeletedAt = DateTime.Now;
                
                var updateAppStoredFileResult = _appStoredFileRepository.Update(appStoredFile);
                var updateStoredFileResult = _storedFileRepository.Update(appStoredFile.StoredFile);
                
                response.Success = updateAppStoredFileResult && updateStoredFileResult;

                if (!response.Success)
                {
                    response.AddError(new ErrorMessage("Failed to delete stored file (soft delete). The database operation was unsuccessful."));
                    await _applicationLogService.AddLogAsync(traceId, "Stored file soft deletion failed because database operation was unsuccessful", ApplicationLogType.Message, ApplicationLogAction.Error);
                }
                else
                {
                    await _applicationLogService.AddLogAsync(traceId, "Stored file soft deleted successfully", ApplicationLogType.Message, ApplicationLogAction.Success);
                }
            }
            else
            {
                // Hard delete - remove completamente do banco
                var removeAppStoredFileResult = _appStoredFileRepository.Remove(appStoredFile);
                response.Success = removeAppStoredFileResult;

                if (!response.Success)
                {
                    response.AddError(new ErrorMessage("Failed to delete stored file. The database operation was unsuccessful."));
                    await _applicationLogService.AddLogAsync(traceId, "Stored file deletion failed because database operation was unsuccessful", ApplicationLogType.Message, ApplicationLogAction.Error);
                }
                else
                {
                    await _applicationLogService.AddLogAsync(traceId, "Stored file deleted successfully", ApplicationLogType.Message, ApplicationLogAction.Success);
                }
            }

            return response;
        }

        public async Task<DefaultResponse> ReprocessFile(int appStoredFileId)
        {
            var traceId = await _applicationLogService.GetTraceId();

            var appStoredFile = _appStoredFileRepository.Get()
                .Include(e => e.AppFile)
                .FirstOrDefault(e => e.Id == appStoredFileId);

            var response = new DefaultResponse(appStoredFile is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage($"Failed to request reprocessing. The stored file with ID {appStoredFileId} was not found."));
                await _applicationLogService.AddLogAsync(traceId, "Reprocess request failed because AppStoredFile was not found", ApplicationLogType.Message, ApplicationLogAction.Error);
                return response;
            }

            appStoredFile.Update(status: (int)AppStoredFileStatusTypes.Processing, statusMessage: AppStoredFileStatusTypes.Processing.GetDescription(), statusDetails: ((int)AppStoredFileStatusTypes.Processing).ToString());

            var updateResult = _appStoredFileRepository.Update(appStoredFile);

            if (!updateResult)
            {
                response.AddError(new ErrorMessage("Failed to update the file for reprocessing. The database operation was unsuccessful."));
                await _applicationLogService.AddLogAsync(traceId, "Reprocess update failed because database operation was unsuccessful", ApplicationLogType.Message, ApplicationLogAction.Error);
                return response;
            }

            this.SetAppStoredFileStatus(appStoredFileId, AppStoredFileStatusTypes.Processing);

            await _applicationLogService.AddLogAsync(traceId, "File marked for reprocessing", ApplicationLogType.Message, ApplicationLogAction.Info);

            var clientDriver = _webSocketWorker.GetDriveClient(appStoredFile.AppFile.UserId);

            if (clientDriver is not null)
            {
                var headers = new Dictionary<string, string>();

                headers.Add("X-Trace-Application-Id", traceId.ToString());

                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "SingleSync",
                    Body = new AppFileUpdateRequestMessage()
                    {
                        AppStoredFileId = appStoredFileId,
                        AppFileId = appStoredFile.AppFileId,
                        Path = appStoredFile.AppFile.Path,
                    },
                    Headers  = headers
                });

                await _applicationLogService.AddLogAsync(traceId, "Reprocess request sent to driver successfully", ApplicationLogType.Message, ApplicationLogAction.Success);
            }
            else
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The driver is not connected."));

                await _applicationLogService.AddLogAsync(traceId, "Reprocess request failed because driver is not connected", ApplicationLogType.Message, ApplicationLogAction.Error);
            }

            return response;
        }

        public async Task<DefaultResponse> SetAppFileStatus(int appFileId, AppFileStatusTypes status)
        {
            var traceId = await _applicationLogService.GetTraceId();
            
            await _applicationLogService.AddLogAsync(traceId, $"Watcher event received for AppFile status update to {status}", ApplicationLogType.Message, ApplicationLogAction.Info, $"Entity: AppFile, ID: {appFileId}, Target Status: {status}");
            
            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == appFileId);
            var response = new DefaultResponse(appFile != null);

            if (!response.Success)
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The file with the specified ID was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Sync request failed because app file was not found", ApplicationLogType.Message, ApplicationLogAction.Error);

                return response;
            }

            appFile.Update(
                status: (int)status,
                statusMessage: status.GetDescription(),
                statusDetails: ((int)status).ToString()
            );

            var updateResult = _appFileRepository.Update(appFile);

            if (updateResult)
            {
                await _applicationLogService.AddLogAsync(traceId, $"AppFile status updated to {status} successfully", ApplicationLogType.Message, ApplicationLogAction.Success, $"Entity: AppFile, ID: {appFileId}, Path: {appFile.Path}, Status: {status}");

                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The update is failed."));
            }
            else
            {
                await _applicationLogService.AddLogAsync(traceId, $"Failed to update AppFile status to {status} because database operation was unsuccessful", ApplicationLogType.Message, ApplicationLogAction.Error);

                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The update is failed."));
            }

            var client = _webSocketWorker.GetWebClient(appFile.UserId);

            if (client is not null)
            {
                _webSocketWorker.SendAsync(client.Id, new WebSocketRequest()
                {
                    Body = null,
                    Event = "AppFileStatusUpdatePing"
                });
            }

            return response;
        }

        public async Task<DefaultResponse> SetAppStoredFileStatus(int appStoredFileId, AppStoredFileStatusTypes status)
        {
            var traceId = await _applicationLogService.GetTraceId();
            
            await _applicationLogService.AddLogAsync(traceId, $"Watcher event received for AppStoredFile status update to {status}", ApplicationLogType.Message, ApplicationLogAction.Info, $"Entity: AppStoredFile, ID: {appStoredFileId}, Target Status: {status}");
            
            var appStoredFile = _appStoredFileRepository.Get().FirstOrDefault(e => e.Id == appStoredFileId);
            var response = new DefaultResponse(appStoredFile != null);

            if (!response.Success)
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The file with the specified ID was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Sync request failed because app file was not found", ApplicationLogType.Message, ApplicationLogAction.Error, $"Entity: AppStoredFile, ID: {appStoredFileId}");

                return response;
            }

            if (appStoredFile.StoredFileId == null && AppStoredFileStatusTypes.Complete == status)
            {
                appStoredFile.Update(
                    status: (int)AppStoredFileStatusTypes.Error,
                    statusMessage: "Error",
                    statusDetails: "The queue not contains file to be proccessed and the file is not saved in system."
                );
            }
            else
            {
                appStoredFile.Update(
                    status: (int)status,
                    statusMessage: status.GetDescription(),
                    statusDetails: ((int)status).ToString()
                );
            }

            var updateResult = _appStoredFileRepository.Update(appStoredFile);


            if (updateResult)
            {
                await _applicationLogService.AddLogAsync(traceId, $"AppStoredFile status updated to {status} successfully", ApplicationLogType.Message, ApplicationLogAction.Success, $"Entity: AppStoredFile, ID: {appStoredFileId}, AppFileId: {appStoredFile.AppFileId}, Status: {status}");
            }
            else
            {
                await _applicationLogService.AddLogAsync(traceId, $"Failed to update AppStoredFile status to {status} because database operation was unsuccessful", ApplicationLogType.Message, ApplicationLogAction.Error);

                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The update is failed."));

                return response;
            }

            var allAppStoredFiles = _appStoredFileRepository.Get().Where(e => e.AppFileId == appStoredFile.AppFileId).ToList();
            var hasProcessing = allAppStoredFiles.Any(e => 
                e.Status == (int)AppStoredFileStatusTypes.Processing || 
                e.Status == null
            );
            var hasPathNotFounded = allAppStoredFiles.Any(e => e.Status == (int)AppStoredFileStatusTypes.PathNotFounded);
            var hasLockedFiles = allAppStoredFiles.Any(e => e.Status == (int)AppStoredFileStatusTypes.LockedFiles);
            var hasErrors = allAppStoredFiles.Any(e => e.Status == (int)AppStoredFileStatusTypes.Error);
            var allComplete = allAppStoredFiles.All(e => 
                e.Status == (int)AppStoredFileStatusTypes.Complete
            );
            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == appStoredFile.AppFileId);
            
            if (appFile != null)
            {
                if (hasProcessing)
                {
                    appFile.Update(status: (int)AppFileStatusTypes.Processing, statusMessage: AppFileStatusTypes.Processing.GetDescription(), statusDetails: ((int)AppFileStatusTypes.Processing).ToString());
                }
                else if (allComplete)
                {
                    appFile.Update(status: (int)AppFileStatusTypes.Synced, statusMessage: AppFileStatusTypes.Synced.GetDescription(), statusDetails: ((int)AppFileStatusTypes.Synced).ToString());
                }
                else if (hasPathNotFounded)
                {
                    appFile.Update(status: (int)AppFileStatusTypes.PathNotFounded, statusMessage: AppFileStatusTypes.PathNotFounded.GetDescription(), statusDetails: ((int)AppFileStatusTypes.PathNotFounded).ToString());
                }
                else if (hasErrors)
                {
                    appFile.Update(status: (int)AppFileStatusTypes.Unsynced, statusMessage: AppFileStatusTypes.Unsynced.GetDescription(), statusDetails: ((int)AppFileStatusTypes.Unsynced).ToString());
                }
                else if (hasLockedFiles)
                {
                    appFile.Update(status: (int)AppFileStatusTypes.LockedFiles, statusMessage: AppFileStatusTypes.LockedFiles.GetDescription(), statusDetails: ((int)AppFileStatusTypes.Unsynced).ToString());
                }

                var appFileUpdateResult = _appFileRepository.Update(appFile);

                if (appFileUpdateResult)
                {
                    await _applicationLogService.AddLogAsync(traceId, $"AppFile status automatically updated based on AppStoredFiles", ApplicationLogType.Message, ApplicationLogAction.Success);
                }
            }

            var client = _webSocketWorker.GetWebClient(appStoredFile.UserId);

            if (client is not null)
            {
                _webSocketWorker.SendAsync(client.Id, new WebSocketRequest()
                {
                    Body = null,
                    Event = "AppFileStatusUpdatePing"
                });
            }

            return response;
        }

        public async Task<DefaultResponse> CheckAppStoredFileStatus(CheckAppStoredFileStatusRequestDto request)
        {
            var traceId = await _applicationLogService.GetTraceId();
            var response = new DefaultResponse(true);
            var appStoredFile = _appStoredFileRepository.Get().Include(e => e.AppFile).FirstOrDefault(e => e.Id == request.AppStoredFileId);

            if (appStoredFile == null)
            {
                response.AddError(new ErrorMessage("AppStoredFile not found."));

                await _applicationLogService.AddLogAsync(traceId, "Status check failed because AppStoredFile was not found", ApplicationLogType.Message, ApplicationLogAction.Error);

                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, "Status check initiated", ApplicationLogType.Message, ApplicationLogAction.Info);

            var clientDriver = _webSocketWorker.GetDriveClient(appStoredFile.UserId);

            if (clientDriver is not null)
            {
                var headers = new Dictionary<string, string>();

                headers.Add("X-Trace-Application-Id", traceId.ToString());

                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "CheckStatus",
                    Headers = headers,
                    Body = new AppFileStatusCheckRequestMessage()
                    {
                        AppStoredFileId = request.AppStoredFileId,
                        Path = appStoredFile.AppFile.Path
                    }
                });

                await _applicationLogService.AddLogAsync(traceId, "Status check request sent to driver successfully", ApplicationLogType.Message, ApplicationLogAction.Success);
            }
            else
            {
                response.AddError(new ErrorMessage("Driver is not connected."));

                await _applicationLogService.AddLogAsync(traceId, "Status check failed because driver is not connected", ApplicationLogType.Message, ApplicationLogAction.Error);
            }

            return response;
        }

        public async Task<DefaultResponse> CheckAppFileStatus(CheckAppFileStatusRequestDto request)
        {
            var traceId = await _applicationLogService.GetTraceId();
            var response = new DefaultResponse(true);
            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == request.AppFileId);

            if (appFile == null)
            {
                response.AddError(new ErrorMessage("AppFile not found."));

                await _applicationLogService.AddLogAsync(traceId, "Status check failed because AppFile was not found", ApplicationLogType.Message, ApplicationLogAction.Error, $"Entity: AppFile, ID: {request.AppFileId}");
                
                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, "Status check flow initiated for checking AppFile status with driver", ApplicationLogType.Message, ApplicationLogAction.Info, $"Entity: AppFile, ID: {request.AppFileId}, Path: {appFile.Path}, Name: {appFile.Name}");

            // Buscar o último arquivo sincronizado
            var lastSyncedFile = _appStoredFileRepository.Get()
                .Include(e => e.StoredFile)
                .Where(e => e.AppFileId == request.AppFileId && e.Status == (int)AppStoredFileStatusTypes.Complete)
                .OrderByDescending(e => e.CreateDate)
                .FirstOrDefault();

            var clientDriver = _webSocketWorker.GetDriveClient(appFile.UserId);

            if (clientDriver is not null)
            {
                var headers = new Dictionary<string, string>();

                headers.Add("X-Trace-Application-Id", traceId.ToString());

                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "CheckAppFileStatusAll",
                    Headers = headers,
                    Body = new AppFileStatusCheckAllRequestMessage()
                    {
                        AppFileId = request.AppFileId,
                        Path = appFile.Path,
                        LastSyncedFileSize = lastSyncedFile?.StoredFile?.SizeInBytes,
                        LastSyncedFileDate = lastSyncedFile?.CreateDate
                    }
                });

                await _applicationLogService.AddLogAsync(traceId, "Status check request sent to driver for AppFile successfully", ApplicationLogType.Message, ApplicationLogAction.Success, $"Entity: AppFile, ID: {request.AppFileId}, Path: {appFile.Path}, LastSyncedFileSize: {lastSyncedFile?.StoredFile?.SizeInBytes}, LastSyncedDate: {lastSyncedFile?.CreateDate}");
            }
            else
            {
                response.AddError(new ErrorMessage("Driver is not connected."));
                await _applicationLogService.AddLogAsync(traceId, "Status check failed because driver is not connected", ApplicationLogType.Message, ApplicationLogAction.Error, $"Entity: AppFile, ID: {request.AppFileId}, Path: {appFile.Path}");
            }

            return response;
        }

        public async Task<BaseResponse<bool>> DriverIsConnected()
        {
            var traceId = await _applicationLogService.GetTraceId();
            var response = new BaseResponse<bool>(true);
            var clients = _webSocketWorker.GetClients();
            var driverConnected = clients.Any(e =>
                e.Value.Context != null &&
                e.Value.Context.TryGetValue("type", out var type) && type == "drive"
            );

            response.Data = driverConnected;

            return response;
        }

        public async Task<DefaultResponse> DeleteSoftDeletedItems()
        {
            var traceId = await _applicationLogService.GetTraceId();
            var response = new DefaultResponse(true);

            try
            {
                // Busca todos os itens marcados como deletados (ignora o filtro global)
                var deletedAppFiles = _dbContext.AppFile
                    .IgnoreQueryFilters()
                    .Where(e => e.IsDeleted)
                    .ToList();

                var deletedAppStoredFiles = _dbContext.AppStoredFile
                    .IgnoreQueryFilters()
                    .Where(e => e.IsDeleted)
                    .Include(e => e.StoredFile)
                    .ToList();

                var deletedStoredFiles = _dbContext.StoredFile
                    .IgnoreQueryFilters()
                    .Where(e => e.IsDeleted)
                    .ToList();

                // Remove permanentemente todos os itens
                _dbContext.AppFile.RemoveRange(deletedAppFiles);
                _dbContext.AppStoredFile.RemoveRange(deletedAppStoredFiles);
                _dbContext.StoredFile.RemoveRange(deletedStoredFiles);

                await _dbContext.SaveChangesAsync();

                var totalDeleted = deletedAppFiles.Count + deletedAppStoredFiles.Count + deletedStoredFiles.Count;

                await _applicationLogService.AddLogAsync(
                    traceId, 
                    $"Permanently deleted {totalDeleted} items ({deletedAppFiles.Count} AppFiles, {deletedAppStoredFiles.Count} AppStoredFiles, {deletedStoredFiles.Count} StoredFiles)", 
                    ApplicationLogType.Message, 
                    ApplicationLogAction.Success
                );
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.AddError(new ErrorMessage($"Failed to delete items permanently: {ex.Message}"));
                await _applicationLogService.AddLogAsync(traceId, "Permanent deletion failed", ApplicationLogType.Message, ApplicationLogAction.Error, ex.Message);
            }

            return response;
        }
    }
}
