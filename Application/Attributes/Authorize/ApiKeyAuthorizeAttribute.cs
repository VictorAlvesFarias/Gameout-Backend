using Microsoft.AspNetCore.Authorization;

namespace ASP.NET_Core_Template.Attributes
{
    public class ApiKeyAuthorizeAttribute : AuthorizeAttribute
    {
        public ApiKeyAuthorizeAttribute()
        {
            AuthenticationSchemes = "ApiKey";
        }
    }
}

