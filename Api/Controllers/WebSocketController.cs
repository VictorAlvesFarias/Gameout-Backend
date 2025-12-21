using Application.Workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Api.Toolkit.Ws.Application.Dtos;
using Web.Api.Toolkit.Ws.Application.Workers;

namespace ASP.NET_Core_Template.Controllers
{
    [ApiController]
    [Route("api/websocket")]
    public class WebSocketOrchestratorController : ControllerBase
    {
        private readonly AppFileWorker _webSocketWorker;
        private readonly ILogger<WebSocketOrchestratorController> _logger;

        public WebSocketOrchestratorController(AppFileWorker webSocketWorker, ILogger<WebSocketOrchestratorController> logger)
        {
            _webSocketWorker = webSocketWorker;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint WebSocket - Client conecta aqui
        /// </summary>
        [HttpGet("/ws")]
        public async Task HandleWebSocket()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsync("WebSocket connection required");
                return;
            }

            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _webSocketWorker.AcceptWebSocketAsync(HttpContext, webSocket, HttpContext.RequestAborted);
        }

        /// <summary>
        /// Obtém convite de conexão WebSocket
        /// Aceita autenticação via JWT (Bearer) ou ApiKey (X-API-KEY header)
        /// </summary>
        [HttpPost("connect")]
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        public IActionResult GetConnectionInfo()
        {
            try
            {
                // Após autenticação, User.Identity.Name ou Claims terão o userId
                var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User?.Identity?.Name 
                          ?? Guid.NewGuid().ToString();

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
        /// Obtém lista de clientes conectados
        /// </summary>
        [HttpGet("clients")]
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        public IActionResult GetConnectedClients()
        {
            try
            {
                var clients = _webSocketWorker.GetClients();
                
                var clientList = clients.Select(c => new
                {
                    clientId = c.Key,
                    instanceId = c.Value.InstanceId,
                    socketState = c.Value.Socket.State.ToString(),
                    headers = c.Value.Headers,
                    cookies = c.Value.Cookies
                }).ToList();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalClients = clientList.Count,
                        clients = clientList
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter lista de clientes");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Erro ao obter lista de clientes conectados"
                });
            }
        }

        /// <summary>
        /// Obtém estatísticas do orquestrador
        /// </summary>
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
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
