namespace Application.Dtos.AppFile
{
    public class CheckAppFileStatusResponseDto
    {
        public bool HasProcessing { get; set; }
        public bool HasError { get; set; }
        public bool HasSizeIssue { get; set; }
        public int ProcessingCount { get; set; }
        public int ErrorCount { get; set; }
        public int SizeIssueCount { get; set; }
        public List<AppFileStatusDetailDto> Details { get; set; } = new List<AppFileStatusDetailDto>();
    }
}
