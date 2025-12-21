using Application.Services.ApiKeyService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Toolkit.Helpers.Api.Extensions;
using Web.Api.Toolkit.Helpers.Application.Dtos;

namespace ASP.NET_Core_Template.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeyController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        [Authorize]
        [HttpPost("generate")]
        public async Task<ActionResult<BaseResponse<string>>> GenerateApiKey()
        {
            var result = await _apiKeyService.GenerateApiKeyAsync();
            return this.Result(result);
        }

        [Authorize]
        [HttpGet("current")]
        public async Task<ActionResult<BaseResponse<string>>> GetCurrentApiKey()
        {
            var result = await _apiKeyService.GetCurrentApiKeyAsync();
            return this.Result(result);
        }
    }
}

