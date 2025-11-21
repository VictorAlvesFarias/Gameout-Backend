using Web.Api.Toolkit.Entity.Domain.Entities;

namespace Domain.Entitites.ApplicationContextDb
{
    public class BaseUserOwnedEntity : BaseEntity
    {
        public ApplicationUser User { get; set; }
        public string UserId { get; set; }
    }
}
