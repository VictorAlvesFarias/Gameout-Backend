namespace Application.Dtos.AppFile
{
    public class AppFileStatusDetailDto
    {
        public int AppStoredFileId { get; set; }
        public int AppFileId { get; set; }
        public string AppFileName { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public string StatusDetails { get; set; }
        public long? OriginalSize { get; set; }
        public long? CurrentSize { get; set; }
        public bool SizeMismatch { get; set; }
    }
}
 