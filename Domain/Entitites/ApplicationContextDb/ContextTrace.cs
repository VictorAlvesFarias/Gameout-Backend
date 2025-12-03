using Web.Api.Toolkit.Entity.Domain.Entities;

namespace Domain.Entitites.ApplicationContextDb
{
    public class ContextTrace : BaseEntity
    {
        public int TraceId { get; set; }
        public Trace Trace { get; set; }
        public string EntityName { get; set; } // Nome da entidade (ex: "AppFile", "AppStoredFile")
        public string EntityId { get; set; } // ID da entidade como string para flexibilidade
    }
}

