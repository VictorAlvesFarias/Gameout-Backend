using Web.Api.Toolkit.Ws.Application.Dtos;

namespace Application.Dtos.WebSocket
{
    public class GetConnectedClientsResponseDto
    {
        public int TotalClients { get; set; }
        public List<WebSocketClient> Clients { get; set; }
    }
}
