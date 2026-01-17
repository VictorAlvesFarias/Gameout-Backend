using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Web.Api.Toolkit.Identity.Application.Configuration;

public static class AuthenticationSetup
{
    public static AuthenticationBuilder AddIdentityAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var JwtAppSettings = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtAppSettings.SecurityKey));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtAppSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtAppSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            RequireExpirationTime = true,
            ValidateLifetime = true
        };

        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = JwtAppSettings.Issuer;
            options.Audience = JwtAppSettings.Audience;
            options.SecurityKey = JwtAppSettings.SecurityKey;
            options.SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
            options.AccessTokenExpiration = JwtAppSettings.AccessTokenExpiration;
        });

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 4;
            options.Password.RequiredUniqueChars = 0;
            options.User.AllowedUserNameCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_-.";
        });

        return services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = tokenValidationParameters;
        });
    }
}
