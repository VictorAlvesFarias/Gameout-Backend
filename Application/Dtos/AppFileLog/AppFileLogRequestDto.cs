using Application.Types;

namespace Application.Dtos.AppFileLog
{
    public class AppFileLogRequestDto
    {
        public int? AppFileId { get; set; }
        public int? AppStoredFileId { get; set; }
        public int? StoredFileId { get; set; }
        public int? ActionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
