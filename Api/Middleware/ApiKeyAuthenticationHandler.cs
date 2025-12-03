using Domain.Entitites.ApplicationContextDb;
using Infrastructure.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ASP.NET_Core_Template.Middleware
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ApplicationDbContext _context;
        private const string ApiKeyHeaderName = "X-API-Key";

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ApplicationDbContext context)
            : base(options, logger, encoder, clock)
        {
            _context = context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return AuthenticateResult.Fail("API Key was not provided");
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return AuthenticateResult.Fail("API Key was not provided");
            }

            // Buscar API key no banco de dados
            var userApiKey = await _context.Set<UserApiKey>()
                .FirstOrDefaultAsync(x => x.ApiKey == providedApiKey && x.IsActive);

            if (userApiKey == null)
            {
                return AuthenticateResult.Fail("Invalid API Key");
            }

            // Atualizar último uso
            userApiKey.LastUsed = DateTime.UtcNow;
            userApiKey.UpdateDate = DateTime.UtcNow;
            _context.Set<UserApiKey>().Update(userApiKey);
            await _context.SaveChangesAsync();

            // Criar claims com o UserId para que o mediator funcione
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userApiKey.UserId),
                new Claim(ClaimTypes.Name, "Driver")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}

