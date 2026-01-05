using Application.Dtos.WebSocket;
using Application.Services.WebSocketService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Toolkit.Helpers.Api.Extensions;
using Web.Api.Toolkit.Helpers.Application.Dtos;

namespace ASP.NET_Core_Template.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebSocketController : ControllerBase
    {
        private readonly IWebSocketService _webSocketService;
        private readonly ILogger<WebSocketController> _logger;

        public WebSocketController(IWebSocketService webSocketService, ILogger<WebSocketController> logger)
        {
            _webSocketService = webSocketService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém convite de conexão WebSocket
        /// Aceita autenticação via JWT (Bearer) ou ApiKey (X-API-KEY header)
        /// </summary>
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        [HttpPost("connect")]
        public async Task<ActionResult<BaseResponse<WebSocketConnectionInfoResponseDto>>> GetConnectionInfo()
        {
            var result = _webSocketService.GetConnectionInfo();
            return this.Result(result);
        }

        /// <summary>
        /// Obtém lista de clientes conectados
        /// </summary>
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        [HttpGet("clients")]
        public async Task<ActionResult<BaseResponse<GetConnectedClientsResponseDto>>> GetConnectedClients()
        {
            var result = _webSocketService.GetConnectedClients();
            return this.Result(result);
        }

        /// <summary>
        /// Obtém estatísticas do orquestrador WebSocket
        /// </summary>
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        [HttpGet("stats")]
        public async Task<ActionResult<BaseResponse<GetWebSocketStatisticsResponseDto>>> GetStatistics()
        {
            var result = _webSocketService.GetStatistics();
            return this.Result(result);
        }
    }
}
