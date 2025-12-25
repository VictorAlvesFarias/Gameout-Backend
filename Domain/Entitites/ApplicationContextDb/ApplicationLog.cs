namespace Domain.Entitites.ApplicationContextDb
{
    public class ApplicationLog : BaseUserOwnedEntity
    {
        public string Message { get; set; }
        public string Details { get; set; }
        public int TraceId { get; set; }
        public Trace Trace { get; set; }
        public int Type { get; set; } // Code=1, Exception=2, Json=3, Message=4, Query=5
        public int Action { get; set; } // Error=1, Warning=2, Success=3, Info=4
    }
}

