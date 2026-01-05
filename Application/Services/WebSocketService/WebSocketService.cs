using Application.Dtos.WebSocket;
using Application.Workers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Security.Claims;
using Web.Api.Toolkit.Helpers.Application.Dtos;
using Web.Api.Toolkit.Ws.Application.Dtos;

namespace Application.Services.WebSocketService
{
    public class WebSocketService : IWebSocketService
    {
        private readonly AppFileWorker _webSocketWorker;
        private readonly ILogger<WebSocketService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebSocketService(AppFileWorker webSocketWorker, ILogger<WebSocketService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _webSocketWorker = webSocketWorker;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DefaultResponse> HandleWebSocketConnectionAsync()
        {
            try
            {
                var webSocket = await _httpContextAccessor.HttpContext.WebSockets.AcceptWebSocketAsync();

                await _webSocketWorker.AcceptWebSocketAsync(_httpContextAccessor.HttpContext, webSocket, _httpContextAccessor.HttpContext.RequestAborted);

                return new DefaultResponse(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aceitar conexão WebSocket");
                var errorResponse = new DefaultResponse(false);
                errorResponse.AddError(new ErrorMessage("Erro ao processar conexão WebSocket"));
                return errorResponse;
            }
        }

        public BaseResponse<WebSocketConnectionInfoResponseDto> GetConnectionInfo()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? _httpContextAccessor.HttpContext.User.Identity?.Name;

                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guid))
                {
                    _logger.LogWarning("Tentativa de conexão WebSocket com userId inválido: {UserId}", userId);
                    var errorResponse = new BaseResponse<WebSocketConnectionInfoResponseDto>(false);
                    errorResponse.AddError(new ErrorMessage("UserId inválido ou não encontrado"));
                    return errorResponse;
                }

                var connectionInfo = _webSocketWorker.GetAvailableInstance(guid);

                var response = new BaseResponse<WebSocketConnectionInfoResponseDto>(true)
                {
                    Data = new WebSocketConnectionInfoResponseDto
                    {
                        Url = connectionInfo.Url,
                        Token = connectionInfo.Token,
                        ExpiresAt = connectionInfo.ExpiresAt
                    }
                };

                _logger.LogInformation(
                    "Convite gerado para usuário {UserId} - URL: {Url}, Expira em: {ExpiresAt}",
                    guid,
                    connectionInfo.Url,
                    connectionInfo.ExpiresAt
                );

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar convite de conexão");
                
                var response = new BaseResponse<WebSocketConnectionInfoResponseDto>(false);
                response.AddError(new ErrorMessage("Erro ao gerar convite de conexão WebSocket"));
                return response;
            }
        }

        public BaseResponse<GetConnectedClientsResponseDto> GetConnectedClients()
        {
            try
            {
                var clients = _webSocketWorker.GetClients();

                var clientList = clients.Select(c => new ConnectedClientResponseDto
                {
                    ClientId = c.Value.Id.ToString(),
                    InstanceId = c.Value.InstanceId,
                    SocketState = c.Value.Socket.State.ToString(),
                    Headers = c.Value.Headers,
                    Cookies = c.Value.Cookies
                }).ToList();

                var response = new BaseResponse<GetConnectedClientsResponseDto>(true)
                {
                    Data = new GetConnectedClientsResponseDto
                    {
                        TotalClients = clientList.Count,
                        Clients = clientList
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter lista de clientes conectados");
                
                var response = new BaseResponse<GetConnectedClientsResponseDto>(false);
                response.AddError(new ErrorMessage("Erro ao obter lista de clientes conectados"));
                return response;
            }
        }

        public BaseResponse<GetWebSocketStatisticsResponseDto> GetStatistics()
        {
            try
            {
                var stats = _webSocketWorker.GetStatistics();

                var response = new BaseResponse<GetWebSocketStatisticsResponseDto>(true)
                {
                    Data = new GetWebSocketStatisticsResponseDto
                    {
                        TotalInstances = stats.TotalInstances,
                        TotalConnections = stats.TotalClients,
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas do WebSocket");
                
                var response = new BaseResponse<GetWebSocketStatisticsResponseDto>(false);
                response.AddError(new ErrorMessage("Erro ao obter estatísticas"));
                return response;
            }
        }

        public async Task BroadcastAsync(WebSocketRequest request)
        {
            try
            {
                await _webSocketWorker.BroadcastAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar broadcast para evento {Event}", request.Event);
                throw;
            }
        }
    }
}
