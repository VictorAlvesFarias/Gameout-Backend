using Microsoft.AspNetCore.Mvc;
using Packages.Ws.Application.Dtos;
using Packages.Ws.Application.Workers;
using System.Security.Claims;

namespace ASP.NET_Core_Template.Controllers
{
    [ApiController]
    [Route("api/websocket")]
    public class WebSocketOrchestratorController : ControllerBase
    {
        private readonly WebSocketWorker _webSocketWorker;
        private readonly ILogger<WebSocketOrchestratorController> _logger;

        public WebSocketOrchestratorController(WebSocketWorker webSocketWorker, ILogger<WebSocketOrchestratorController> logger)
        {
            _webSocketWorker = webSocketWorker;
            _logger = logger;
        }

        /// <summary>
        /// Obtém informações de conexão WebSocket (URL + token)
        /// </summary>
        [HttpPost("connect")]
        public IActionResult GetConnectionInfo()
        {
            try
            {
                // Obter userId do usuário autenticado ou de cookies
                var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Request.Cookies["id"] ?? Guid.NewGuid().ToString();

                if (!Guid.TryParse(userId, out var guid))
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Erro ao gerar convite de conexão WebSocket"
                    });
                }

                var connectionInfo = _webSocketWorker.GetAvailableInstance(guid);

                _webSocketWorker.BroadcastAsync(new WebSocketRequest()
                {
                    Event = "Teste"
                });

                _logger.LogInformation(
                    "Convite gerado para usuário {UserId} - URL: {Url}, Expira em: {ExpiresAt}",
                    userId,
                    connectionInfo.Url,
                    connectionInfo.ExpiresAt
                );

                return Ok(new
                {
                    success = true,
                    data = connectionInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar convite de conexão");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Erro ao gerar convite de conexão WebSocket"
                });
            }
        }

        /// <summary>
        /// Obtém estatísticas do orquestrador
        /// </summary>
        [HttpGet("stats")]
        public IActionResult GetStatistics()
        {
            try
            {
                _webSocketWorker.BroadcastAsync(new WebSocketRequest()
                {
                    Event = "Teste"
                });
                var stats = _webSocketWorker.GetStatistics();
                return Ok(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Erro ao obter estatísticas"
                });
            }
        }
    }
}
