using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Web.Api.Toolkit.Ws.Application.Dtos;
using Web.Api.Toolkit.Ws.Application.Workers;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Web.Api.Toolkit.Entity.Infraestructure.Repositories;
using Domain.Entitites.ApplicationContextDb;
using System.IO.Compression;

namespace Application.Workers
{
    public class AppFileWorker : WebSocketWorker
    {
        private readonly IServiceProvider _services;  

        public AppFileWorker (
            ILogger<AppFileWorker> logger, 
            IConfiguration configuration,
            IServiceProvider services
        ) : 
        base (
            logger, 
            configuration.GetValue<int>("WebSocket:MaxConnectionsPerInstance", 100), 
            configuration.GetValue<int>("WebSocket:InviteExpirationMinutes", 5)
        ) 
        {
            _services = services;
        }

        protected override WebSocketAuthResponse Authentication(Dictionary<string, string> headers, Dictionary<string, string> cookies)
        {
            var x = base.Authentication(headers, cookies);

            return x;
        }

        protected override ValidateInviteTokenResult ValidateInviteToken(string token)
        {
            var x =  base.ValidateInviteToken(token);

            return x;
        }

        protected override Task OnClientConnectedAsync(WebSocketClient client)
        {
            if (client.Cookies.Contains(new KeyValuePair<string, string>("type", "drive")))
            {
                using var scope = _services.CreateScope();

                var userRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<ApplicationUser>>();
                var apiKeyRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<UserApiKey>>();
                var userId = apiKeyRepo.Get().FirstOrDefault(x => x.ApiKey.Equals(client.Headers.GetValueOrDefault("X-API-Key")))?.UserId;
                var webClient = GetClients().FirstOrDefault(e =>
                    e.Value.Cookies.Any(c => c.Value == userId && c.Key == "id") &&
                    e.Value.Cookies.Any(c => c.Value == "web" && c.Key == "type")
                ).Value;

                if (webClient is null)
                {
                    return Task.CompletedTask;
                }

                SendAsync(webClient.Id, new WebSocketRequest()
                {
                    Event = "DriveStatusUpdated",
                });

                client.Cookies.Add("id", userId);
            }

            return base.OnClientConnectedAsync(client);
        }

        protected override Task OnClientDisconnectedAsync(WebSocketClient client)
        {
            if (client.Cookies.Contains(new KeyValuePair<string, string>("type", "drive")))
            {

                using var scope = _services.CreateScope();

                var userRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<ApplicationUser>>();
                var apiKeyRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<UserApiKey>>();
                var userId = apiKeyRepo.Get().FirstOrDefault(x => x.ApiKey.Equals(client.Headers.GetValueOrDefault("X-API-Key")))?.UserId;
                var webClient = GetClients().FirstOrDefault(e =>
                    e.Value.Cookies.Any(c => c.Value == userId && c.Key == "id") &&
                    e.Value.Cookies.Any(c => c.Value == "web" && c.Key == "type")
                ).Value;

                if (webClient is null)
                {
                    return Task.CompletedTask;
                }

                SendAsync(webClient.Id, new WebSocketRequest()
                {
                    Event = "DriveStatusUpdated",
                });

                client.Cookies.Add("id", userId);
            }

            return base.OnClientDisconnectedAsync(client);
        }
    }
}
