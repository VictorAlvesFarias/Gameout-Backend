namespace Application.Configuration
{
    public class WebSocketOptions
    {
        public const string SectionName = "WebSocketOptions";
        public int MaxConnectionsPerInstance { get; set; }
        public int InviteExpirationMinutes { get; set; }
    }
}
