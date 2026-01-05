namespace Application.Dtos.WebSocket
{
    public class ConnectedClientResponseDto
    {
        public string ClientId { get; set; }
        public string InstanceId { get; set; }
        public string SocketState { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, string> Cookies { get; set; }
    }
}
