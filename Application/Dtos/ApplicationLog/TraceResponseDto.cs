namespace Application.Dtos.ApplicationLog
{
    public class TraceResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UserId { get; set; }
        public int LogsCount { get; set; }
    }
}

