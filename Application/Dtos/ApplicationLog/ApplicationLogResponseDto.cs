namespace Application.Dtos.ApplicationLog
{
    public class ApplicationLogResponseDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public int TraceId { get; set; }
        public string Type { get; set; }
        public string Action { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UserId { get; set; }
    }
}

