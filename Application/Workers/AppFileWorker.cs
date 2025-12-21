using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Web.Api.Toolkit.Ws.Application.Dtos;
using Web.Api.Toolkit.Ws.Application.Workers;

namespace Application.Workers
{
    public class AppFileWorker : WebSocketWorker
    {
        public AppFileWorker(ILogger<AppFileWorker> logger, IConfiguration configuration) : base(
            logger,
            configuration.GetValue<int>("WebSocket:MaxConnectionsPerInstance", 100),
            configuration.GetValue<int>("WebSocket:InviteExpirationMinutes", 5)
        )
        {
        }

        protected override WebSocketAuthResponse Authentication(Dictionary<string, string> headers, Dictionary<string, string> cookies)
        {
            return base.Authentication(headers, cookies);
        }

        protected override ValidateInviteTokenResult ValidateInviteToken(string token)
        {
            return base.ValidateInviteToken(token);
        }
    }
}
