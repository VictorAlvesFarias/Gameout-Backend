using Microsoft.Extensions.Logging;
using Application.Configuration;
using Microsoft.Extensions.Options;
using Web.Api.Toolkit.Ws.Application.Dtos;
using Web.Api.Toolkit.Ws.Application.Workers;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Web.Api.Toolkit.Entity.Infraestructure.Repositories;
using Domain.Entitites.ApplicationContextDb;
using System.IO.Compression;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Web.Api.Toolkit.Identity.Application.Configuration;
using Microsoft.AspNetCore.Authentication;

namespace Application.Workers
{
    public class AppFileWorker : WebSocketWorker
    {
        private readonly IServiceProvider _services;
        private readonly JwtOptions _jwtOptions;

        public AppFileWorker (
            ILogger<AppFileWorker> logger, 
            IOptions<WebSocketOptions> webSocketOptions,
            IOptions<JwtOptions> jwtOptions,
            IServiceProvider services
        ) : 
        base (
            logger, 
            webSocketOptions.Value.MaxConnectionsPerInstance
        ) 
        {
            _services = services;
            _jwtOptions = jwtOptions.Value;
        }

        protected override WebSocketAuthResponse Authentication(WebSocketClient client)
        {
            var userId = string.Empty;

            if (client.Context == null)
            {
                client.Context = new Dictionary<string, string>();
            }

            if (!client.HttpContext.Request.Cookies.TryGetValue("type", out var clientType))
            {
                return new WebSocketAuthResponse()
                {
                    Success = false,
                    Message = "Client type not provided"
                };
            }

            if (clientType == "drive")
            {
                var authResult = client.HttpContext.AuthenticateAsync("ApiKey").GetAwaiter().GetResult();

                if (!authResult.Succeeded)
                {
                    return new WebSocketAuthResponse()
                    {
                        Success = false,
                        Message = $"API Key authentication failed: {authResult.Failure?.Message ?? "Invalid API Key"}"
                    };
                }

                userId = authResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }
            else if (clientType == "web")
            {
                if (!client.HttpContext.Request.Cookies.TryGetValue("token", out var token) || string.IsNullOrWhiteSpace(token))
                {
                    return new WebSocketAuthResponse()
                    {
                        Success = false,
                        Message = "JWT token not provided"
                    };
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtOptions.SecurityKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return new WebSocketAuthResponse()
                    {
                        Success = false,
                        Message = "Invalid JWT token"
                    };
                }
            }
            else
            {
                return new WebSocketAuthResponse()
                {
                    Success = false,
                    Message = "Invalid client type"
                };
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return new WebSocketAuthResponse()
                {
                    Success = false,
                    Message = "User ID not found"
                };
            }

            client.Context.Add("type", clientType);
            client.Context.Add("userId", userId);

            return new WebSocketAuthResponse()
            {
                Success = true,
                Message = $"{clientType} authenticated successfully"
            };
        }

        protected override async Task OnClientConnectedAsync(WebSocketClient client)
        {
            var clients = this.GetClients();
            
            if (client.Context.TryGetValue("type", out var type) && type == "drive")
            {
                if (client.Context.TryGetValue("userId", out var userId))
                {
                    var webClient = clients.FirstOrDefault(e =>
                        e.Value.Context != null &&
                        e.Value.Context.TryGetValue("type", out var clientType) && clientType == "web" &&
                        e.Value.Context.TryGetValue("userId", out var webUserId) && webUserId == userId
                    ).Value;

                    if (webClient != null)
                    {
                        await SendAsync(webClient.Id, new WebSocketRequest()
                        {
                            Event = "DriveStatusUpdated",
                            Body = new
                            {
                                Status = "connected",
                                DriveClientId = client.Id.ToString(),
                                UserId = userId,
                                ConnectedAt = DateTime.UtcNow
                            }
                        });
                    }
                }
            }
            else if (client.Context.TryGetValue("type", out var webType) && webType == "web")
            {
                if (client.Context.TryGetValue("userId", out var userId))
                {
                    var driveClient = clients.FirstOrDefault(e =>
                        e.Value.Context != null &&
                        e.Value.Context.TryGetValue("type", out var clientType) && clientType == "drive" &&
                        e.Value.Context.TryGetValue("userId", out var driveUserId) && driveUserId == userId
                    ).Value;

                    if (driveClient != null)
                    {
                        await SendAsync(client.Id, new WebSocketRequest()
                        {
                            Event = "DriveStatusUpdated",
                            Body = new
                            {
                                Status = "connected",
                                DriveClientId = driveClient.Id.ToString(),
                                UserId = userId,
                                ConnectedAt = DateTime.UtcNow
                            }
                        });
                    }
                }
            }

            await base.OnClientConnectedAsync(client);
        }

        protected override async Task OnClientDisconnectedAsync(WebSocketClient client)
        {
            if (client.Context != null && client.Context.TryGetValue("type", out var type) && type == "drive")
            {
                // Buscar o cliente web com o mesmo userId
                if (client.Context.TryGetValue("userId", out var userId))
                {
                    var clients = GetClients();
                    var webClient = clients.FirstOrDefault(e =>
                        e.Value.Context != null &&
                        e.Value.Context.TryGetValue("type", out var clientType) && clientType == "web" &&
                        e.Value.Context.TryGetValue("userId", out var webUserId) && webUserId == userId
                    ).Value;

                    if (webClient != null)
                    {
                        await SendAsync(webClient.Id, new WebSocketRequest()
                        {
                            Event = "DriveStatusUpdated",
                            Body = new
                            {
                                Status = "disconnected",
                                DriveClientId = client.Id.ToString(),
                                UserId = userId,
                                DisconnectedAt = DateTime.UtcNow
                            }
                        });
                    }
                }
            }

            await base.OnClientDisconnectedAsync(client);
        }

        public WebSocketClient GetDriveClient(string userId)
        {
            var clients = this.GetClients();
            return clients.FirstOrDefault(e =>
                e.Value.Context != null &&
                e.Value.Context.TryGetValue("type", out var type) && type == "drive" &&
                e.Value.Context.TryGetValue("userId", out var clientUserId) && clientUserId == userId
            ).Value;
        }

        public WebSocketClient GetWebClient(string userId)
        {
            var clients = this.GetClients();
            return clients.FirstOrDefault(e =>
                e.Value.Context != null &&
                e.Value.Context.TryGetValue("type", out var type) && type == "web" &&
                e.Value.Context.TryGetValue("userId", out var clientUserId) && clientUserId == userId
            ).Value;
        }

    }
}
