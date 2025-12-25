using Application.Types;

namespace Application.Dtos.ApplicationLog
{
    public class ApplicationLogRequestDto
    {
        public string Message { get; set; }
        public string Details { get; set; }
        public ApplicationLogType Type { get; set; }
        public ApplicationLogAction Action { get; set; }
    }
}
