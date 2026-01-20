namespace Application.Dtos.AppFile
{
    public class AppStoredFileResponseDto
    {
        public int Id { get; set; }
        public int AppFileId { get; set; }
        public int? StoredFileId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public double? SizeInBytes { get; set; }
    }
}
