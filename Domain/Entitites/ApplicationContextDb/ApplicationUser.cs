using Microsoft.AspNetCore.Identity;

namespace Domain.Entitites.ApplicationContextDb
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool Deleted { get; set; }
        public string Name { get; set; }
    }
}