using Application.Dtos.AppFile;
using Application.Services.AppFileLogService;
using Domain.Entitites.ApplicationContextDb;
using Domain.Entitites.Shared;
using Domain.Queues.AppFileDtos;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Packages.Entity.Infraestructure.Repositories;
using Packages.Helpers.Application.Dtos;
using Packages.Queues.Application.Services;
using Packages.Ws.Application.Dtos;
using Packages.Ws.Application.Workers;
using System.IO.Compression;

namespace Application.Services.AppFileService
{
    public class AppFileService : IAppFileService
    {
        private readonly IBaseRepository<AppFile> _appFileRepository;
        private readonly IBaseRepository<AppStoredFile> _appStoredFileRepository;
        private readonly IBaseRepository<StoredFile> _storedFileRepository;
        private readonly ApplicationContext _applicationContext;
        private readonly WebSocketWorker _webSocketWorker;
        private readonly IAppFileLogService _appFileLogService;

        public AppFileService(
            IBaseRepository<AppFile> appFileRepository,
            IBaseRepository<StoredFile> storedFileRepository,
            IBaseRepository<AppStoredFile> appStoredFileRepository,
            ApplicationContext applicationContext,
            WebSocketWorker webSocketWorker,
            IAppFileLogService appFileLogService
        )
        {
            _appFileRepository = appFileRepository;
            _storedFileRepository = storedFileRepository;
            _appStoredFileRepository = appStoredFileRepository;
            _applicationContext = applicationContext;
            _webSocketWorker = webSocketWorker;
            _appFileLogService = appFileLogService;
        }

        public BaseResponse<AppFile> InsertFile(AppFile req)
        {
            var appFileAddResult = _appFileRepository.AddAsync(req).Result;
            var response = new BaseResponse<AppFile>(appFileAddResult is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to insert file. Please check the provided data and try again."));
                _appFileLogService.LogActionAsync(
                    AppFileActionType.InsertFile,
                    "File insertion failed - database operation unsuccessful",
                    appFileId: req.Id,
                    path: req.Path,
                    recordName: req.Name
                );
            }
            else
            {
                _appFileLogService.LogActionAsync(
                    AppFileActionType.InsertFile,
                    "File inserted successfully",
                    appFileId: req.Id,
                    path: req.Path,
                    recordName: req.Name
                );
            }

            response.Data = req;

            return response;
        }

        public BaseResponse<List<AppFileResponseDto>> GetFiles()
        {
            var result = _appFileRepository.Get().OrderByDescending(e => e.Id).ToList();
            var response = new BaseResponse<List<AppFileResponseDto>>(result is not null);
            var responseMapper = (AppFile e) => new AppFileResponseDto()
            {
                Id = e.Id,
                Name = e.Name,
                Path = e.Path,
                VersionControl = e.VersionControl,
                Observer = e.Observer,
                CreateDate = e.CreateDate,
                UpdateDate = e.UpdateDate,
                Synced = e.Synced,
                AutoValidateSync = e.AutoValidateSync
            };

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to retrieve files. Please try again later."));
            }
            else
            {
                response.Data = result.Select(responseMapper).ToList();
            }

            return response;
        }

        public BaseResponse<StoredFile> DownloadFile(int id)
        {
            var appStoredFile = _appStoredFileRepository.Get().FirstOrDefault(e => e.Id == id);
            var result = _storedFileRepository.Get().FirstOrDefault(e => e.Id == appStoredFile.StoredFileId);
            var response = new BaseResponse<StoredFile>(result is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to download file. The file may not exist or may be corrupted."));
                _appFileLogService.LogActionAsync(
                    AppFileActionType.DownloadFile,
                    "File download failed - file not found or corrupted",
                    appFileId: appStoredFile?.AppFileId,
                    appStoredFileId: id,
                    storedFileId: appStoredFile?.StoredFileId,
                    path: appStoredFile?.AppFile?.Path,
                    recordName: appStoredFile?.AppFile?.Name
                );
            }
            else
            {
                _appFileLogService.LogActionAsync(
                    AppFileActionType.DownloadFile,
                    "File download completed successfully",
                    appFileId: appStoredFile?.AppFileId,
                    appStoredFileId: id,
                    storedFileId: appStoredFile?.StoredFileId,
                    path: appStoredFile?.AppFile?.Path,
                    recordName: appStoredFile?.AppFile?.Name
                );
                response.Data = result;
            }

            return response;
        }

        public BaseResponse<AppFile> UpdateFile(AppFile req, int id)
        {
            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == id);
            var response = new BaseResponse<AppFile>(appFile is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to update file. The file with the specified ID was not found."));
                _appFileLogService.LogActionAsync(
                    AppFileActionType.UpdateFile,
                    "File update failed - file not found",
                    appFileId: id,
                    path: req.Path,
                    recordName: req.Name
                );

                return response;
            }

            appFile.Update(req.Name, req.Path, req.VersionControl, req.Observer, req.AutoValidateSync);

            response.Success = _appFileRepository.Update(appFile);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to update file. Database operation was unsuccessful."));
                _appFileLogService.LogActionAsync(
                    AppFileActionType.UpdateFile,
                    "File update failed - database operation unsuccessful",
                    appFileId: id,
                    path: req.Path,
                    recordName: req.Name
                );

                return response;
            }

            _appFileLogService.LogActionAsync(
                AppFileActionType.UpdateFile,
                "File updated successfully",
                appFileId: id,
                path: req.Path,
                recordName: req.Name
            );

            var clientDriver = _webSocketWorker.GetClients().FirstOrDefault(e =>
                e.Value.Headers.Any(e => e.Value == appFile.UserId && e.Key == "id") &&
                e.Value.Headers.Any(e => e.Value == "drive" && e.Key == "type")
            ).Value;

            _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
            {
                Event = "SetEvents",
                Body = ""
            });

            response.Data = req;

            return response;
        }

        public BaseResponse<List<AppStoredFileResponseDto>> GetAppStoredFiles(int? idAppFile = null, bool? processing = false)
        {
            var appStoredFilesQuery = _appStoredFileRepository
                .Get()
                .Include(e => e.StoredFile)
                .Include(e => e.AppFile)
                .Where(e => (e.AppFileId == idAppFile || idAppFile == null) && e.Processing == processing);

            IQueryable<AppStoredFile> finalQuery;

            if (!(processing ?? false))
            {
                finalQuery = appStoredFilesQuery.Where(e => e.StoredFileId != null).Select(e => new AppStoredFile()
                {
                    StoredFile = new StoredFile
                    {
                        Id = e.StoredFile.Id,
                        Name = e.StoredFile.Name,
                        MimeType = e.StoredFile.MimeType,
                        CreateDate = e.StoredFile.CreateDate,
                        UpdateDate = e.StoredFile.UpdateDate,
                        SizeInBytes = e.StoredFile.SizeInBytes,
                        Base64 = null
                    },
                    AppFileId = e.AppFileId,
                    CreateDate = e.CreateDate,
                    UpdateDate = e.UpdateDate,
                    Error = e.Error,
                    Id = e.Id,
                });
            }
            else
            {
                finalQuery = appStoredFilesQuery;
            }

            var appStoredFiles = finalQuery.ToList();
            var response = new BaseResponse<List<AppStoredFileResponseDto>>(appStoredFiles is not null);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to retrieve stored files. Please try again later."));
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
                        Error = e.Error,
                        Message = e.Message,
                        StoredFileId = e.StoredFileId,
                        Processing = e.Processing,
                        Versioned = e.Versioned,
                        Name = e?.AppFile?.Name,
                        Path = e?.AppFile?.Path,
                        SizeInBytes = e?.StoredFile?.SizeInBytes
                    }
                ).ToList();
            }

            return response;
        }

        public DefaultResponse RequestSync(int idAppFile)
        {
            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == idAppFile);
            var response = new DefaultResponse(appFile != null);

            if (!response.Success)
            {
                response.Errors.Add(new ErrorMessage("Failed to request synchronization. The file with the specified ID was not found."));
                _appFileLogService.LogActionAsync(
                    AppFileActionType.RequestSync,
                    "Sync request failed - file not found",
                    appFileId: idAppFile
                );

                return response;
            }

            _appFileLogService.LogActionAsync(
                AppFileActionType.RequestSync,
                "Sync request initiated",
                appFileId: idAppFile,
                path: appFile.Path,
                recordName: appFile.Name
            );

            var appStoredFile = new AppStoredFile
            {
                AppFileId = idAppFile,
                Processing = true,
                Versioned = false
            };
            var appStoredFileAddResult = _appStoredFileRepository.AddAsync(appStoredFile).Result;

            if (appStoredFileAddResult is null)
            {
                response.Errors.Add(new ErrorMessage("Failed to create stored file record. Database operation was unsuccessful."));
                response.Success = false;

                return response;
            }

            var clientDriver = _webSocketWorker.GetClients().FirstOrDefault(e => 
                e.Value.Headers.Any(e => e.Value == appFile.UserId && e.Key == "id") &&
                e.Value.Headers.Any(e => e.Value == "drive" && e.Key == "type")
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

            var client = _webSocketWorker.GetClients().FirstOrDefault(e =>
                e.Value.Headers.Any(e => e.Value == appFile.UserId && e.Key == "id") &&
                e.Value.Headers.Any(e => e.Value == "web" && e.Key == "type")
            ).Value;

            if (client is not null)
            {
                _webSocketWorker.SendAsync(client.Id, new WebSocketRequest()
                {
                    Body = null,
                    Event = "NewsFilesRequestPing"
                });
            }

            _appFileLogService.LogActionAsync(
                AppFileActionType.RequestSync,
                "Sync request completed",
                appFileId: idAppFile,
                path: appFile.Path,
                recordName: appFile.Name
            );

            return response;
        }

        public DefaultResponse SingleSync(AppFileUpdateResponseMessag req)
        {
            var response = new DefaultResponse();

            //Latest app file created 
            var appStoredFile = _appStoredFileRepository.Get()
                .Include(e => e.AppFile)
                .FirstOrDefault(e => e.Id == req.AppStoredFileId);

            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == appStoredFile.AppFileId);

            if (appStoredFile is null)
            {
                response.AddError(new ErrorMessage("Failed to perform single sync. The app file record was not found."));

                _appFileLogService.LogActionAsync(
                    AppFileActionType.SingleSync,
                    "Single sync failed - AppFile not found",
                    appStoredFileId: req.AppStoredFileId
                );

                return response;
            }

            //Old latest app file created 
            var appStoredFileOld = _appStoredFileRepository.Get()
              .Where(e => e.Processing == false && e.AppFileId == appFile.Id)
              .Include(e => e.AppFile)
              .OrderByDescending(e => e.CreateDate)
              .FirstOrDefault();

            var storedFile = new StoredFile()
            {
                Base64 = req.MemoryStream,
                Name = appStoredFile.AppFile.Name,
                MimeType = "application/zip",
                SizeInBytes = req.UncompressedSize
            };
            var addedStoredFile = _storedFileRepository.AddAsync(storedFile).Result;

            if (appStoredFile is null)
            {
                response.AddError(new ErrorMessage("Failed to perform single sync. The stored file record was not found."));

                _appFileLogService.LogActionAsync(
                    AppFileActionType.SingleSync,
                    "Single sync failed - AppStoredFile not found",
                    appStoredFileId: req.AppStoredFileId
                );

                return response;
            }

            _appFileLogService.LogActionAsync(
                AppFileActionType.StreamAssigned,
                "Stream assigned for processing",
                appFileId: appStoredFile.AppFileId,
                appStoredFileId: req.AppStoredFileId,
                path: appStoredFile.AppFile.Path,
                recordName: appStoredFile.AppFile.Name
            );

            appStoredFile.Update(storedFileId: addedStoredFile.Id, processing: false);
            appFile.Update(synced: true);

            var appStoredFileUpdatedResult = _appStoredFileRepository.Update(appStoredFile);
            var appFileUpdatedResult = _appFileRepository.Update(appFile);

            if (!appStoredFileUpdatedResult || !appFileUpdatedResult)
            {
                response.AddError(new ErrorMessage("Failed to update stored file record. Database operation was unsuccessful."));

                return response;
            }

            if (appFile.VersionControl)
            {
                appStoredFileOld.Update(versioned: true);

                var odlAppStoredFileUpdatedResult = _appStoredFileRepository.Update(appStoredFileOld);

                if (!odlAppStoredFileUpdatedResult)
                {
                    response.AddError(new ErrorMessage("Failed to update app stored file record. Database operation was unsuccessful."));

                    return response;
                }
            }
            else
            {
                var odlAppStoredFileUpdatedResult = _appStoredFileRepository.Remove(appStoredFileOld);

                if (!odlAppStoredFileUpdatedResult)
                {
                    response.AddError(new ErrorMessage("Failed to remove app stored file record. Database operation was unsuccessful."));

                    return response;
                }
            }

            var client = _webSocketWorker.GetClients().FirstOrDefault(e =>
                e.Value.Headers.Any(e => e.Value == appFile.UserId && e.Key == "id") &&
                e.Value.Headers.Any(e => e.Value == "web" && e.Key == "type")
            ).Value;

            if (client is not null)
            {
                _webSocketWorker.SendAsync(client.Id, new WebSocketRequest()
                {
                    Body = null,
                    Event = "AppFileUpdatedPing"
                });
            }

            _appFileLogService.LogActionAsync(
                AppFileActionType.ProcessingCompleted,
                "Processing completed successfully",
                appFileId: appStoredFile.AppFileId,
                appStoredFileId: req.AppStoredFileId,
                storedFileId: addedStoredFile.Id,
                path: appStoredFile.AppFile.Path,
                recordName: appStoredFile.AppFile.Name
            );

            return response;
        }

        public DefaultResponse DeleteFile(int id)
        {
            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == id);
            var removeAppFileResult = _appFileRepository.Remove(appFile);
            var response = new DefaultResponse(removeAppFileResult);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to delete file. The file may not exist or you may not have permission to delete it."));
                _appFileLogService.LogActionAsync(
                    AppFileActionType.DeleteFile,
                    "File deletion failed - database operation unsuccessful",
                    appFileId: id,
                    path: appFile?.Path,
                    recordName: appFile?.Name
                );
            }
            else
            {
                _appFileLogService.LogActionAsync(
                    AppFileActionType.DeleteFile,
                    "File deleted successfully",
                    appFileId: id,
                    path: appFile?.Path,
                    recordName: appFile?.Name
                );
            }

            return response;
        }

        public DefaultResponse DeleteStoredFile(int id)
        {
            var appStoredFile = _appStoredFileRepository.Get().FirstOrDefault(e => e.Id == id);
            var removeAppFileResult = _appStoredFileRepository.Remove(appStoredFile);
            var response = new DefaultResponse(removeAppFileResult);

            if (!response.Success)
            {
                response.AddError(new ErrorMessage("Failed to delete stored file. The stored file may not exist or you may not have permission to delete it."));
                _appFileLogService.LogActionAsync(
                    AppFileActionType.DeleteStoredFile,
                    "Stored file deletion failed - database operation unsuccessful",
                    appFileId: appStoredFile?.AppFileId,
                    appStoredFileId: id,
                    storedFileId: appStoredFile?.StoredFileId,
                    path: appStoredFile?.AppFile?.Path,
                    recordName: appStoredFile?.AppFile?.Name
                );
            }
            else
            {
                _appFileLogService.LogActionAsync(
                    AppFileActionType.DeleteStoredFile,
                    "Stored file deleted successfully",
                    appFileId: appStoredFile?.AppFileId,
                    appStoredFileId: id,
                    storedFileId: appStoredFile?.StoredFileId,
                    path: appStoredFile?.AppFile?.Path,
                    recordName: appStoredFile?.AppFile?.Name
                );
            }

            return response;
        }

        public DefaultResponse ProcessError(AppFileErrorMessage errorMessage)
        {
            var response = new DefaultResponse(true);
            var appStoredFile = _appStoredFileRepository.Get().Include(e => e.AppFile).FirstOrDefault(e => e.Id == errorMessage.AppStoredFileId);

            if (appStoredFile != null)
            {
                appStoredFile.Update(
                    processing: true,
                    mensagem: errorMessage.Mensagem,
                    error: errorMessage.Error
                );

                _appStoredFileRepository.Update(appStoredFile);

                _appFileLogService.LogActionAsync(
                    AppFileActionType.ProcessingError,
                    $"Processing error: {errorMessage.Mensagem}",
                    appFileId: appStoredFile.AppFileId,
                    appStoredFileId: errorMessage.AppStoredFileId,
                    path: appStoredFile.AppFile.Path,
                    recordName: appStoredFile.AppFile.Name
                );

                // Notificar via WebSocket
                var client = _webSocketWorker.GetClients().FirstOrDefault(e =>
                    e.Value.Headers.Any(e => e.Value == appStoredFile.UserId && e.Key == "id") &&
                    e.Value.Headers.Any(e => e.Value == "web" && e.Key == "type")
                ).Value;

                if (client is not null)
                {
                    _webSocketWorker.SendAsync(client.Id, new WebSocketRequest()
                    {
                        Body = null,
                        Event = "AppFileErrorPing"
                    });
                }
            }

            return response;
        }

        public DefaultResponse ReprocessFile(int appStoredFileId)
        {
            var response = new DefaultResponse(true);
            var appStoredFile = _appStoredFileRepository.Get()
                .Include(e => e.AppFile)
                .FirstOrDefault(e => e.Id == appStoredFileId);

            if (appStoredFile == null)
            {
                response.AddError(new ErrorMessage("Arquivo não encontrado."));
                return response;
            }

            // Limpar erro e marcar para reprocessamento
            appStoredFile.Update(
                processing: true,
                mensagem: null,
                error: null
            );

            var updateResult = _appStoredFileRepository.Update(appStoredFile);
            if (!updateResult)
            {
                response.AddError(new ErrorMessage("Falha ao atualizar o arquivo para reprocessamento."));
                return response;
            }

            var clientDriver = _webSocketWorker.GetClients().FirstOrDefault(e =>
                e.Value.Headers.Any(e => e.Value == appStoredFile.UserId && e.Key == "Authorization") &&
                e.Value.Headers.Any(e => e.Value == "Driver" && e.Key == "Type")
            ).Value;

            if (clientDriver is not null)
            {
                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "SingleSync",
                    Body = new AppFileUpdateRequestMessage()
                    {
                        AppStoredFileId = appStoredFileId,
                        Path = appStoredFile.AppFile.Path
                    }
                });
            }

            _appFileLogService.LogActionAsync(
                AppFileActionType.RequestSync,
                "File marked for reprocessing",
                appFileId: appStoredFile.AppFileId,
                appStoredFileId: appStoredFileId,
                path: appStoredFile.AppFile.Path,
                recordName: appStoredFile.AppFile.Name
            );

            return response;
        }

        public DefaultResponse DeleteFileWithError(int appStoredFileId)
        {
            var response = new DefaultResponse();
            var appStoredFile = _appStoredFileRepository.Get()
                .Include(e => e.AppFile)
                .FirstOrDefault(e => e.Id == appStoredFileId);

            if (appStoredFile == null)
            {
                response.AddError(new ErrorMessage("Arquivo não encontrado."));
                return response;
            }

            // Remover o StoredFile se existir
            if (appStoredFile.StoredFileId.HasValue)
            {
                var storedFile = _storedFileRepository.Get().FirstOrDefault(e => e.Id == appStoredFile.StoredFileId.Value);
                if (storedFile != null)
                {
                    _storedFileRepository.Remove(storedFile);
                }
            }

            // Remover o AppStoredFile
            var removeResult = _appStoredFileRepository.Remove(appStoredFile);
            if (!removeResult)
            {
                response.AddError(new ErrorMessage("Falha ao excluir o arquivo."));
                return response;
            }

            _appFileLogService.LogActionAsync(
                AppFileActionType.DeleteStoredFile,
                "File with error deleted",
                appFileId: appStoredFile.AppFileId,
                appStoredFileId: appStoredFileId,
                path: appStoredFile.AppFile.Path,
                recordName: appStoredFile.AppFile.Name
            );

            response.Success = true;
            return response;
        }

        public DefaultResponse CheckProcessingStatus(int appStoredFileId)
        {
            var response = new DefaultResponse(true);
            var appStoredFile = _appStoredFileRepository.Get()
                .FirstOrDefault(e => e.Id == appStoredFileId);

            var clientDriver = _webSocketWorker.GetClients().FirstOrDefault(e =>
                e.Value.Headers.Any(e => e.Value == appStoredFile.UserId && e.Key == "Authorization") &&
                e.Value.Headers.Any(e => e.Value == "Driver" && e.Key == "Type")
            ).Value;

            if (clientDriver is not null)
            {
                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "IsProcessing",
                    Body = new AppFileStatusCheckRequestMessage()
                    {
                        AppStoredFileId = appStoredFileId,
                        RequestId = Guid.NewGuid().ToString()
                    }
                });
            }

            return response;
        }

        public DefaultResponse ProcessStatusResponse(AppFileStatusCheckResponseMessage statusResponse)
        {
            var response = new DefaultResponse(true);
            var appStoredFile = _appStoredFileRepository.Get().Include(e => e.AppFile).FirstOrDefault(e => e.Id == statusResponse.AppStoredFileId);

            if (appStoredFile != null && appStoredFile.Processing)
            {
                // Se tem erro, atualizar o status
                if (!string.IsNullOrEmpty(statusResponse.Error))
                {
                    appStoredFile.Update(
                        processing: true,
                        mensagem: statusResponse.Message,
                        error: statusResponse.Error
                    );
                    _appStoredFileRepository.Update(appStoredFile);

                    _appFileLogService.LogActionAsync(
                        AppFileActionType.ProcessingError,
                        $"Status check error: {statusResponse.Message}",
                        appFileId: appStoredFile.AppFileId,
                        appStoredFileId: statusResponse.AppStoredFileId,
                        path: appStoredFile.AppFile.Path,
                        recordName: appStoredFile.AppFile.Name
                    );
                }

                var client = _webSocketWorker.GetClients().FirstOrDefault(e =>
                    e.Value.Headers.Any(e => e.Value == appStoredFile.UserId && e.Key == "id") &&
                    e.Value.Headers.Any(e => e.Value == "web" && e.Key == "type")
                ).Value;

                if (client is not null)
                {
                    _webSocketWorker.SendAsync(client.Id, new WebSocketRequest()
                    {
                        Body = null,
                        Event = "AppFileStatusUpdatePing"
                    });
                }
            }

            return response;
        }

        public DefaultResponse StatusUpdate(AppFileValidateStatusResponse req)
        {
            var response = new DefaultResponse(true);
            var allAppFiles = _appFileRepository.Get().ToList();
            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == req.AppFileId);

            if (appFile is null)
            {
                response.AddError(new ErrorMessage("App file not found"));

                return response;
            }

            var appStoredFile = _appStoredFileRepository.Get()
                .Where(e => e.AppFileId == appFile.Id && e.Processing == false)
                .OrderByDescending(e => e.CreateDate)
                .FirstOrDefault();

            if (appStoredFile is null)
            {
                response.AddError(new ErrorMessage("No processed stored file found for the specified app file"));
                return response;
            }

            var sizeInBytes = _storedFileRepository.Get().Where(e => e.Id == appStoredFile.StoredFileId).Select(e => e.SizeInBytes).FirstOrDefault();

            appFile.Update(synced: sizeInBytes.Equals(req.SizeInBytes));

            var updateResult = _appFileRepository.Update(appFile);

            if (!updateResult)
            {
                response.AddError(new ErrorMessage("Failed to update app file status. Database operation was unsuccessful."));
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
                    Event = "AppFileStatusUpdatePing"
                });
            }

            return response;
        }

        public DefaultResponse RequestStatusUpdate(int appFileId)
        {
            var response = new DefaultResponse(true);
            var appFile = _appFileRepository.Get().FirstOrDefault(e => e.Id == appFileId);

            if (appFile is null)
            {
                response.AddError(new ErrorMessage("App file not found"));

                return response;
            }

            var clients = _webSocketWorker.GetClients();
                
            var clientDriver = clients.FirstOrDefault(e =>
                e.Value.Headers.Any(e => e.Value.Equals(appFile.UserId) && e.Key.Equals("id")) &&
                e.Value.Headers.Any(e => e.Value.Equals("drive") && e.Key.Equals("type"))
            ).Value;

            if (clientDriver is not null)
            {
                _webSocketWorker.SendAsync(clientDriver.Id, new WebSocketRequest()
                {
                    Event = "ValidateSync",
                    Body = new AppFileValidateStatusRequest()
                    {
                        AppFile = appFile
                    }
                });
            }
            return response;
        }
    }
}
