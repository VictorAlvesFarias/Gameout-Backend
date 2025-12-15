using Application.Dtos.AppFile;
using Application.Extensions;
using Application.Services.ApplicationLogService;
using Application.Types;
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
using Web.Api.Toolkit.Ws.Application.Workers;

namespace Application.Services.AppFileService
{
    public class AppFileService : IAppFileService
    {
        private readonly IBaseRepository<AppFile> _appFileRepository;
        private readonly IBaseRepository<AppStoredFile> _appStoredFileRepository;
        private readonly IBaseRepository<StoredFile> _storedFileRepository;
        private readonly ApplicationContext _applicationContext;
        private readonly WebSocketWorker _webSocketWorker;
        private readonly IApplicationLogService _applicationLogService;

        public AppFileService(
            IBaseRepository<AppFile> appFileRepository,
            IBaseRepository<StoredFile> storedFileRepository,
            IBaseRepository<AppStoredFile> appStoredFileRepository,
            ApplicationContext applicationContext,
            WebSocketWorker webSocketWorker,
            IApplicationLogService applicationLogService
        )
        {
            _appFileRepository = appFileRepository;
            _storedFileRepository = storedFileRepository;
            _appStoredFileRepository = appStoredFileRepository;
            _applicationContext = applicationContext;
            _webSocketWorker = webSocketWorker;
            _applicationLogService = applicationLogService;
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
                Status = (int)AppFileStatusTypes.Pending
            };
            var appFileAddResult = await _appFileRepository.AddAsync(appFile);
            var response = new BaseResponse<AppFileResponseDto>(appFileAddResult is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to insert file. Please check the provided data and try again."));

                await _applicationLogService.AddLogAsync(traceId, "File inserted unsuccessfully", "Message", "Error");
            }
            else
            {
                await _applicationLogService.AddLogAsync(traceId, "File inserted successfully", "Message", "Success");

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

        public async Task<BaseResponse<StoredFile>> DownloadFile(int id)
        {
            var traceId = await _applicationLogService.GetTraceId();

            await _applicationLogService.AddContextTraceAsync(traceId, "AppStoredFile", id.ToString());

            var appStoredFile = _appStoredFileRepository.Get().FirstOrDefault(e => e.Id == id);
            var result = _storedFileRepository.Get().FirstOrDefault(e => e.Id == appStoredFile.StoredFileId);
            var response = new BaseResponse<StoredFile>(result is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to download file. The file may not exist or may be corrupted."));

                await _applicationLogService.AddLogAsync(traceId, "File inserted unsuccessfully", "Message", "Error");
            }
            else
            {
                await _applicationLogService.AddLogAsync(traceId, "File inserted successfully", "Message", "Successs");

                response.Data = result;
            }

            return response;
        }

        public async Task<BaseResponse<AppFileResponseDto>> UpdateFile(AppFileRequestDto req, int id)
        {
            var traceId = await _applicationLogService.GetTraceId();

            await _applicationLogService.AddContextTraceAsync(traceId, "AppFile", id.ToString());

            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == id);
            var response = new BaseResponse<AppFileResponseDto>(appFile is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to update file. The file with the specified ID was not found."));

                await _applicationLogService.AddLogAsync(traceId, "File update failed - file not found", "Message", "Error");
                

                return response;
            }

            appFile.Update(req.Name, req.Path, req.VersionControl, req.Observer, req.AutoValidateSync);

            response.Success = _appFileRepository.Update(appFile);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to update file. Database operation was unsuccessful."));

                await _applicationLogService.AddLogAsync(traceId, "File update failed - database operation unsuccessful", "Message", "Error");

                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, "File updated successfully", "Message", "Success");

            var clients = _webSocketWorker.GetClients();
            var clientDriver = clients.FirstOrDefault(e =>
                e.Value.Headers.Any(e => e.Value.Equals(appFile.UserId) && e.Key.Equals("id")) &&
                e.Value.Headers.Any(e => e.Value.Equals("drive") && e.Key.Equals("type"))
            ).Value;

            if (clientDriver is not null)
            {
                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "SetEvents",
                    Body = ""
                });
            }
            else
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The driver is not connected."));
               
                await _applicationLogService.AddLogAsync(traceId, "Sync request failed - driver not connected", "Message", "Error");

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

            _applicationLogService.AddContextTraceAsync(traceId, "AppFile", idAppFile.ToString()).Wait();

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
                            e.Status == (int)AppStoredFileStatusTypes.PendingWithError ||
                            e.Status == null
                        )
                    );
            }

            var appStoredFiles = finalQuery.ToList();
            var response = new BaseResponse<List<AppStoredFileResponseDto>>(appStoredFiles is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to retrieve stored files. Please try again later."));

                _applicationLogService.AddLogAsync(traceId, "Failed to retrieve stored files. Please try again later.", "Message", "Error").Wait();
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

            await _applicationLogService.AddContextTraceAsync(traceId, "AppFile", req.IdAppFile.ToString());

            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == req.IdAppFile);
            var response = new DefaultResponse(appFile != null);

            if (!response.Success)
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The file with the specified ID was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Sync request failed - file not found", "Message", "Error");

                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, "Sync request initiated", "Message", "Info");

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

                await _applicationLogService.AddLogAsync(traceId, "Failed to create stored file record", "Message", "Error");
               
                return response;
            }

            this.SetAppFileStatus(req.IdAppFile, AppFileStatusTypes.Pending, "Processing");

            var clients = _webSocketWorker.GetClients();
            var clientDriver = clients.FirstOrDefault(e =>
                e.Value.Headers.Any(e => e.Value.Equals(appFile.UserId) && e.Key.Equals("id")) &&
                e.Value.Headers.Any(e => e.Value.Equals("drive") && e.Key.Equals("type"))
            ).Value;

            if (clientDriver is not null)
            {
                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "SingleSync",
                    Body = new AppFileUpdateRequestMessage()
                    {
                        AppStoredFileId = appStoredFileAddResult.Id,
                        Path = appFile.Path
                    }
                });
            }
            else
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The driver is not connected."));
                
                await _applicationLogService.AddLogAsync(traceId, "Sync request failed - driver not connected", "Message", "Error");

                return response;
            }

            var allClients = _webSocketWorker.GetClients();
            var client = allClients.FirstOrDefault(e =>
                e.Value.Cookies.Any(e => e.Value == appFile.UserId && e.Key == "id") &&
                e.Value.Cookies.Any(e => e.Value == "web" && e.Key == "type")
            ).Value;

            if (client is not null)
            {
                _webSocketWorker.SendAsync(client.Id, new WebSocketRequest()
                {
                    Body = null,
                    Event = "NewsFilesRequestPing"
                });
            }

            await _applicationLogService.AddLogAsync(traceId, "Sync request sent to driver", "Message", "Info");

            return response;
        }

        public async Task<DefaultResponse> SingleSync(AppFileStreamFileRequestDto req)
        {
            var traceId = await _applicationLogService.GetTraceId();

            await _applicationLogService.AddContextTraceAsync(traceId, "AppStoredFile", req.AppStoredFileId.ToString());

            var response = new DefaultResponse();
            var appStoredFile = _appStoredFileRepository.Get().Include(e => e.AppFile).FirstOrDefault(e => e.Id == req.AppStoredFileId);

            if (appStoredFile is null)
            {
                response.AddError(new ErrorMessage("Failed to perform single sync. The app stored file record was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Single sync failed - AppStoredFile not found", "Message", "Error");

                return response;
            }

            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == appStoredFile.AppFileId);

            if (appFile is null)
            {
                response.AddError(new ErrorMessage("Failed to perform single sync. The app file record was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Single sync failed - AppFile not found", "Message", "Error");

                return response;
            }

            using var memoryStream = new MemoryStream();

            req.File.CopyTo(memoryStream);

            var appStoredFileOld = _appStoredFileRepository.Get()
                .Where(e => e.Status == (int)AppStoredFileStatusTypes.Complete && e.AppFileId == appFile.Id)
                .Include(e => e.AppFile)
                .OrderByDescending(e => e.CreateDate)
                .FirstOrDefault();

            var storedFile = new StoredFile()
            {
                Bytes = memoryStream.ToArray(),
                Name = appStoredFile.AppFile.Name,
                MimeType = "application/zip",
                SizeInBytes = req.OriginalFileSize
            };
            var addedStoredFile = _storedFileRepository.AddAsync(storedFile).Result;

            if (appStoredFile is null)
            {
                response.AddError(new ErrorMessage("Failed to perform single sync. The stored file record was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Single sync failed - AppStoredFile not found", "Message", "Error");

                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, "Stream received from driver", "Message", "Info");

            appStoredFile.Update(storedFileId: addedStoredFile.Id, status: (int)AppStoredFileStatusTypes.Complete, statusMessage: "Complete");

            var appStoredFileUpdatedResult = _appStoredFileRepository.Update(appStoredFile);

            if (!appStoredFileUpdatedResult)
            {
                response.AddError(new ErrorMessage("Failed to update stored file record. Database operation was unsuccessful."));

                return response;
            }

            if (this.GetAppStoredFiles(appFile.Id, true).Data.Count() == 0)
            {
                appFile.Update(status: (int)AppFileStatusTypes.Synced, statusMessage: "Synced");
                
                var appFileUpdatedResult = _appFileRepository.Update(appFile);

                if (!appFileUpdatedResult)
                {
                    response.AddError(new ErrorMessage("Failed to update stored file record. Database operation was unsuccessful."));

                    return response;
                }
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
                var odlAppStoredFileUpdatedResult = _appStoredFileRepository.Remove(appStoredFileOld);

                if (!odlAppStoredFileUpdatedResult)
                {
                    response.AddError(new ErrorMessage("Failed to remove app stored file record. Database operation was unsuccessful."));

                    return response;
                }
            }

            var allClients = _webSocketWorker.GetClients();
            var client = allClients.FirstOrDefault(e =>
                e.Value.Cookies.Any(e => e.Value == appFile.UserId && e.Key == "id") &&
                e.Value.Cookies.Any(e => e.Value == "web" && e.Key == "type")
            ).Value;

            if (client is not null)
            {
                _webSocketWorker.SendAsync(client.Id, new WebSocketRequest()
                {
                    Body = null,
                    Event = "AppFileUpdatedPing"
                });
            }

            await _applicationLogService.AddLogAsync(traceId, "Processing completed successfully", "Message", "Success");

            return response;
        }

        public async Task<DefaultResponse> DeleteFile(int id)
        {
            var traceId = await _applicationLogService.GetTraceId();

            await _applicationLogService.AddContextTraceAsync(traceId, "AppFile", id.ToString());

            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == id);
            var response = new DefaultResponse(appFile is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage($"Falha ao excluir o arquivo. O arquivo com o ID {id} não foi encontrado."));
                await _applicationLogService.AddLogAsync(traceId, "File deletion failed - file not found", "Message", "Error");
                return response;
            }

            var removeAppFileResult = _appFileRepository.Remove(appFile);
            response.Success = removeAppFileResult;

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Falha ao excluir o arquivo. A operação no banco de dados não foi bem-sucedida."));
                await _applicationLogService.AddLogAsync(traceId, "File deletion failed - database operation unsuccessful", "Message", "Error");
            }
            else
            {
                await _applicationLogService.AddLogAsync(traceId, "File deleted successfully", "Message", "Success");
            }

            return response;
        }

        public async Task<DefaultResponse> DeleteStoredFile(int id)
        {
            var traceId = await _applicationLogService.GetTraceId();

            await _applicationLogService.AddContextTraceAsync(traceId, "AppStoredFile", id.ToString());

            var appStoredFile = _appStoredFileRepository.Get().FirstOrDefault(e => e.Id == id);
            var response = new DefaultResponse(appStoredFile is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage($"Failed to delete stored file. The record with ID {id} was not found."));
                await _applicationLogService.AddLogAsync(traceId, "Stored file deletion failed - record not found", "Message", "Error");
                return response;
            }

            var removeAppStoredFileResult = _appStoredFileRepository.Remove(appStoredFile);

            if (!removeAppStoredFileResult)
            {
                response.AddError(new ErrorMessage("Failed to delete stored file. The database operation was unsuccessful."));
                await _applicationLogService.AddLogAsync(traceId, "Stored file deletion failed - database operation unsuccessful", "Message", "Error");
            }
            else
            {
                await _applicationLogService.AddLogAsync(traceId, "Stored file deleted successfully", "Message", "Success");
            }

            return response;
        }

        public async Task<DefaultResponse> ReprocessFile(int appStoredFileId)
        {
            var traceId = await _applicationLogService.GetTraceId();

            await _applicationLogService.AddContextTraceAsync(traceId, "AppStoredFile", appStoredFileId.ToString());

            var appStoredFile = _appStoredFileRepository.Get()
                .Include(e => e.AppFile)
                .FirstOrDefault(e => e.Id == appStoredFileId);

            var response = new DefaultResponse(appStoredFile is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage($"Failed to request reprocessing. The stored file with ID {appStoredFileId} was not found."));
                await _applicationLogService.AddLogAsync(traceId, "Reprocess request failed - AppStoredFile not found", "Message", "Error");
                return response;
            }

            await _applicationLogService.AddContextTraceAsync(traceId, "AppFile", appStoredFile.AppFileId.ToString());

            appStoredFile.Update(status: (int)AppStoredFileStatusTypes.Processing, statusMessage: "Complete");

            var updateResult = _appStoredFileRepository.Update(appStoredFile);

            if (!updateResult)
            {
                response.AddError(new ErrorMessage("Failed to update the file for reprocessing. The database operation was unsuccessful."));
                await _applicationLogService.AddLogAsync(traceId, "Reprocess update failed - database operation unsuccessful", "Message", "Error");
                return response;
            }

            this.SetAppStoredFileStatus(appStoredFileId, AppStoredFileStatusTypes.Processing, "Processing");

            await _applicationLogService.AddLogAsync(traceId, "File marked for reprocessing", "Message", "Info");

            var clients = _webSocketWorker.GetClients();
            var clientDriver = clients.FirstOrDefault(e =>
                e.Value.Headers.Any(h => h.Value.Equals(appStoredFile.AppFile.UserId) && h.Key.Equals("id")) &&
                e.Value.Headers.Any(h => h.Value.Equals("drive") && h.Key.Equals("type"))
            ).Value;

            if (clientDriver is not null)
            {
                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "SingleSync",
                    Body = new AppFileUpdateRequestMessage()
                    {
                        AppStoredFileId = appStoredFileId,
                        Path = appStoredFile.AppFile.Path,
                    }
                });

                await _applicationLogService.AddLogAsync(traceId, "Reprocess request sent to driver", "Message", "Info");
            }
            else
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The driver is not connected."));

                await _applicationLogService.AddLogAsync(traceId, "Reprocess request failed - driver not connected", "Message", "Error");
            }

            return response;
        }

        public async Task<DefaultResponse> SetAppFileStatus(int appFileId, AppFileStatusTypes status, string statusMessage, string statusDetails = "")
        {
            var traceId = await _applicationLogService.GetTraceId();
         
            await _applicationLogService.AddContextTraceAsync(traceId, "AppFile", appFileId.ToString());

            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == appFileId);
            var response = new DefaultResponse(appFile != null);

            if (!response.Success)
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The file with the specified ID was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Sync request failed - app file not found", "Message", "Error");

                return response;
            }

            appFile.Update(
                status: (int)status,
                statusMessage: statusMessage,
                statusDetails: statusDetails
            );

            var updateResult = _appFileRepository.Update(appFile);

            if (updateResult)
            {
                await _applicationLogService.AddLogAsync(traceId, $"AppFile status updated to {status}", "Message", "Info");

                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The update is failed."));
            }
            else
            {
                await _applicationLogService.AddLogAsync(traceId, $"Failed to update AppFile status to {status} - database operation unsuccessful", "Message", "Error");

                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The update is failed."));
            }

            var allClients = _webSocketWorker.GetClients();
            var client = allClients.FirstOrDefault(e =>
                e.Value.Cookies.Any(e => e.Value == appFile.UserId && e.Key == "id") &&
                e.Value.Cookies.Any(e => e.Value == "web" && e.Key == "type")
            ).Value;

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

        public async Task<DefaultResponse> SetAppStoredFileStatus(int appStoredFileId, AppStoredFileStatusTypes status, string statusMessage = "", string statusDetails = "")
        {
            var traceId = await _applicationLogService.GetTraceId();
         
            await _applicationLogService.AddContextTraceAsync(traceId, "AppStoredFile", appStoredFileId.ToString());

            var appStoredFile = _appStoredFileRepository.Get().FirstOrDefault(e => e.Id == appStoredFileId);
            var response = new DefaultResponse(appStoredFile != null);

            if (!response.Success)
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The file with the specified ID was not found."));

                await _applicationLogService.AddLogAsync(traceId, "Sync request failed - app file not found", "Message", "Error");

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
                    statusMessage: statusMessage,
                    statusDetails: statusDetails
                );
            }

            var updateResult = _appStoredFileRepository.Update(appStoredFile);


            if (updateResult)
            {
                await _applicationLogService.AddLogAsync(traceId, $"AppFile status updated to {status}", "Message", "Info");
            }
            else
            {
                await _applicationLogService.AddLogAsync(traceId, $"Failed to update AppFile status to {status} - database operation unsuccessful", "Message", "Error");

                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The update is failed."));
            }

            var allClients = _webSocketWorker.GetClients();
            var client = allClients.FirstOrDefault(e =>
                e.Value.Cookies.Any(e => e.Value == appStoredFile.UserId && e.Key == "id") &&
                e.Value.Cookies.Any(e => e.Value == "web" && e.Key == "type")
            ).Value;

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

            await _applicationLogService.AddContextTraceAsync(traceId, "AppStoredFile", request.AppStoredFileId.ToString());

            var response = new DefaultResponse(true);
            var appStoredFile = _appStoredFileRepository.Get().Include(e => e.AppFile).FirstOrDefault(e => e.Id == request.AppStoredFileId);

            if (appStoredFile == null)
            {
                response.AddError(new ErrorMessage("AppStoredFile not found."));

                await _applicationLogService.AddLogAsync(traceId, "Status check failed - AppStoredFile not found", "Message", "Error");

                return response;
            }

            await _applicationLogService.AddLogAsync(traceId, "Status check initiated", "Message", "Info");

            var clients = _webSocketWorker.GetClients();
            var clientDriver = clients.FirstOrDefault(e =>
                e.Value.Headers.Any(h => h.Value.Equals(appStoredFile.UserId) && h.Key.Equals("id")) &&
                e.Value.Headers.Any(h => h.Value.Equals("drive") && h.Key.Equals("type"))
            ).Value;

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

                await _applicationLogService.AddLogAsync(traceId, "Status check request sent to driver", "Message", "Info");
            }
            else
            {
                response.AddError(new ErrorMessage("Driver is not connected."));

                await _applicationLogService.AddLogAsync(traceId, "Status check failed - driver not connected", "Message", "Error");
            }

            return response;
        }
    }
}
