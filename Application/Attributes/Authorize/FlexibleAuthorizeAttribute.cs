using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace ASP.NET_Core_Template.Attributes
{
    public class FlexibleAuthorizeAttribute : AuthorizeAttribute
    {
        public FlexibleAuthorizeAttribute()
        {
            // Permite tanto ApiKey quanto Bearer Token
            AuthenticationSchemes = $"ApiKey,{JwtBearerDefaults.AuthenticationScheme}";
        }
    }
}
