namespace Application.Dtos.AppFile
{
    public class AppStoredFileResponseDto
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public int AppFileId { get; set; }
        public int? StoredFileId { get; set; }
        public bool Versioned { get; set; }
        public bool Processing { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
        public bool Erro { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public double? SizeInBytes { get; set; }
        public string UserId { get; set; }
    }
}
