namespace Application.Dtos.WebSocket
{
    public class GetConnectedClientsResponseDto
    {
        public int TotalClients { get; set; }
        public List<ConnectedClientResponseDto> Clients { get; set; }
    }
}
