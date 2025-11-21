namespace Application.Dtos.AppFileLog
{
    public class AppFileLogResponseDto
    {
        public int Id { get; set; }
        public int? AppFileId { get; set; }
        public int? AppStoredFileId { get; set; }
        public int? StoredFileId { get; set; }
        public string Path { get; set; }
        public string RecordName { get; set; }
        public string ActionMessage { get; set; }
        public int ActionType { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UserId { get; set; }
    }
}
