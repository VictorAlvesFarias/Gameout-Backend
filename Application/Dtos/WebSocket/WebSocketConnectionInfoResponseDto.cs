namespace Application.Dtos.WebSocket
{
    public class WebSocketConnectionInfoResponseDto
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
