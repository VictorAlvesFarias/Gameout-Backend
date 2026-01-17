using System.Security.Claims;
using Web.Api.Toolkit.Helpers.Application.Dtos;

namespace Application.Services.DownloadSignatureService
{
    public interface IDownloadSignatureService
    {
        BaseResponse<string> GenerateSignedDownloadUrl(int fileId);
        BaseResponse<ClaimsPrincipal> ValidateAndExtractClaims(string token);
    }
}
