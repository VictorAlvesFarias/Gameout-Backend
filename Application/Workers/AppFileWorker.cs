using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using Web.Api.Toolkit.Ws.Application.Dtos;
using Web.Api.Toolkit.Ws.Application.Workers;

namespace Application.Workers
{
    public class AppFileWorker : WebSocketWorker
    {
        public AppFileWorker(ILogger<WebSocketWorker> logger, string prefix = "http://localhost:8081/ws/", bool isOrchestrator = true, int maxConnectionsPerInstance = 1, int inviteExpirationMinutes = 5, string baseUrl = "ws://localhost") : base(logger, prefix, isOrchestrator, maxConnectionsPerInstance, inviteExpirationMinutes, baseUrl)
        {
        }

        protected override WebSocketAuthResponse Authentication(WebSocket ws, Dictionary<string, string> headers, Dictionary<string, string> cookies)
        {
            return base.Authentication(ws, headers, cookies);
        }

        protected override ValidateInviteTokenResult ValidateInviteToken(string token)
        {
            return base.ValidateInviteToken(token);
        }
    }
}
