using Application.Dtos.WebSocket;
using Web.Api.Toolkit.Helpers.Application.Dtos;
using Web.Api.Toolkit.Ws.Application.Dtos;

namespace Application.Services.WebSocketService
{
    public interface IWebSocketService
    {
        Task<DefaultResponse> HandleWebSocketConnectionAsync();
        BaseResponse<WebSocketConnectionInfoResponseDto> GetConnectionInfo();
        BaseResponse<GetConnectedClientsResponseDto> GetConnectedClients();
        BaseResponse<GetWebSocketStatisticsResponseDto> GetStatistics();
        Task BroadcastAsync(WebSocketRequest request);
    }
}
