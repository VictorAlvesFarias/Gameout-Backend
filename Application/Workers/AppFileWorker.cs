using Microsoft.Extensions.Logging;
using Packages.Ws.Application.Dtos;
using Packages.Ws.Application.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Application.Workers
{
    public class AppFileWorker : WebSocketWorker
    {
        public AppFileWorker(ILogger<WebSocketWorker> logger, string prefix = "http://localhost:8081/ws/", bool isOrchestrator = true, int maxConnectionsPerInstance = 1, int inviteExpirationMinutes = 5, string baseUrl = "ws://localhost") : base(logger, prefix, isOrchestrator, maxConnectionsPerInstance, inviteExpirationMinutes, baseUrl)
        {
        }

        public override WebSocketAuthResponse Authentication(WebSocket ws, Dictionary<string, string> headers, Dictionary<string, string> cookies)
        {
            return base.Authentication(ws, headers, cookies);
        }

        public override ValidateInviteTokenResult ValidateInviteToken(string token)
        {
            return base.ValidateInviteToken(token);
        }
    }
}
