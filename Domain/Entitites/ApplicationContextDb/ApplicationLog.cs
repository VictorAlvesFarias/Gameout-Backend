namespace Domain.Entitites.ApplicationContextDb
{
    public class ApplicationLog : BaseUserOwnedEntity
    {
        public string Message { get; set; }
        public int TraceId { get; set; }
        public Trace Trace { get; set; }
        public string Type { get; set; } // Code, Exception, Json, Message, etc
        public string Action { get; set; } // Error, Warning, Success, Info, etc
    }
}

