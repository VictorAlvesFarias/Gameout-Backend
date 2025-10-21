namespace Application.Dtos.AppFile
{
    public class StoredFileResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        public byte[] Base64 { get; set; }
        public double SizeInBytes { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UserId { get; set; }
    }
}
