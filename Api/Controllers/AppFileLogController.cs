using Application.Dtos.AppFileLog;
using Application.Services.AppFileLogService;
using Application.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Core_Template.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppFileLogController : ControllerBase
    {
        private readonly IAppFileLogService _appFileLogService;

        public AppFileLogController(IAppFileLogService appFileLogService)
        {
            _appFileLogService = appFileLogService;
        }

        /// <summary>
        /// Busca logs de ações realizadas nos arquivos
        /// </summary>
        /// <param name="appFileId">ID do arquivo (opcional)</param>
        /// <param name="appStoredFileId">ID do arquivo armazenado (opcional)</param>
        /// <param name="storedFileId">ID do arquivo de armazenamento (opcional)</param>
        /// <param name="actionType">Tipo de ação (opcional)</param>
        /// <param name="startDate">Data de início (opcional)</param>
        /// <param name="endDate">Data de fim (opcional)</param>
        /// <param name="userId">ID do usuário (opcional)</param>
        /// <param name="page">Página (padrão: 1)</param>
        /// <param name="pageSize">Tamanho da página (padrão: 50)</param>
        /// <returns>Lista de logs filtrados</returns>
        [HttpGet]
        public async Task<IActionResult> GetLogs(
            [FromQuery] int? appFileId = null,
            [FromQuery] int? appStoredFileId = null,
            [FromQuery] int? storedFileId = null,
            [FromQuery] int? actionType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? userId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var filter = new AppFileLogFilterDto
            {
                AppFileId = appFileId,
                AppStoredFileId = appStoredFileId,
                StoredFileId = storedFileId,
                ActionType = actionType.HasValue ? (AppFileActionType?)actionType.Value : null,
                StartDate = startDate,
                EndDate = endDate,
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _appFileLogService.GetLogsAsync(filter);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
