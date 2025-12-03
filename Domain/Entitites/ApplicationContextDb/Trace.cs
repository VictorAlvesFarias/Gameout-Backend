namespace Domain.Entitites.ApplicationContextDb
{
    public class Trace : BaseUserOwnedEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ApplicationLog> Logs { get; set; } = new List<ApplicationLog>();
        public List<ContextTrace> ContextTraces { get; set; } = new List<ContextTrace>();
    }
}

