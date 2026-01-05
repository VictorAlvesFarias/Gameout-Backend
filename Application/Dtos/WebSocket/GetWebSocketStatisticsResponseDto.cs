namespace Application.Dtos.WebSocket
{
    public class GetWebSocketStatisticsResponseDto
    {
        public int TotalInstances { get; set; }
        public int TotalConnections { get; set; }
        public int ActiveConnections { get; set; }
        public Dictionary<string, object> AdditionalStats { get; set; }
    }
}
