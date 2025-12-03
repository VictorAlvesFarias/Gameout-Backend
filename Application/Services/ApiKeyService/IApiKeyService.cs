using Web.Api.Toolkit.Helpers.Application.Dtos;

namespace Application.Services.ApiKeyService
{
    public interface IApiKeyService
    {
        Task<BaseResponse<string>> GenerateApiKeyAsync();
        Task<BaseResponse<string>> GetCurrentApiKeyAsync();
    }
}

