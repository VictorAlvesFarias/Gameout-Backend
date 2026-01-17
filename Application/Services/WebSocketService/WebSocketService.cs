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

        public BaseResponse<GetConnectedClientsResponseDto> GetConnectedClients()
        {
            try
            {
                var clients = _webSocketWorker.GetClients();
                var response = new BaseResponse<GetConnectedClientsResponseDto>(true)
                {
                    Data = new GetConnectedClientsResponseDto
                    {
                        TotalClients = clients.Count,
                        Clients = clients.Select(e => e.Value).ToList()
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
    }
}
