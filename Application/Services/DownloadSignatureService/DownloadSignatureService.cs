using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Web.Api.Toolkit.Helpers.Application.Dtos;
using Web.Api.Toolkit.Identity.Application.Configuration;

namespace Application.Services.DownloadSignatureService
{
    public class DownloadSignatureService : IDownloadSignatureService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly DownloadOptions _downloadOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DownloadSignatureService
        (
            IOptions<JwtOptions> jwtOptions,
            IOptions<DownloadOptions> downloadOptions,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _jwtOptions = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _downloadOptions = downloadOptions.Value ?? throw new ArgumentNullException(nameof(downloadOptions));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            if (string.IsNullOrEmpty(_jwtOptions.SecurityKey))
            {
                throw new InvalidOperationException("JwtOptions.SecurityKey is not configured");
            }
        }

        public BaseResponse<string> GenerateSignedDownloadUrl(int fileId)
        {
            var response = new BaseResponse<string>();
            var httpContext = _httpContextAccessor.HttpContext;
                
            if (httpContext == null)
            {
                response.AddError(new ErrorMessage("HTTP context is not available"));
                    
                return response;
            }

            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
            if (string.IsNullOrEmpty(userId))
            {
                response.AddError(new ErrorMessage("User not authenticated", 401));
                    
                return response;
            }

            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}";
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtOptions.SecurityKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("fileId", fileId.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim("purpose", "download")
                }),
                Expires = DateTime.UtcNow.AddMinutes(_downloadOptions.ExpirationMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            var signedUrl = $"{baseUrl}/download-file-signed?token={Uri.EscapeDataString(tokenString)}";
                
            response.Data = signedUrl;
    
            return response;
        }

        public BaseResponse<ClaimsPrincipal> ValidateAndExtractClaims(string token)
        {
            var response = new BaseResponse<ClaimsPrincipal>();

            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    response.AddError(new ErrorMessage("Token is required", 401));
                    
                    return response;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtOptions.SecurityKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    response.AddError(new ErrorMessage("Invalid token", 401));

                    return response;
                }

                var purposeClaim = principal.FindFirst("purpose")?.Value;
                
                if (purposeClaim != "download")
                {
                    response.AddError(new ErrorMessage("Invalid token purpose", 401));
                
                    return response;
                }

                response.Data = principal;
            }
            catch (SecurityTokenExpiredException)
            {
                response.AddError(new ErrorMessage("Token has expired", 401));
            }
            catch (SecurityTokenException ex)
            {
                response.AddError(new ErrorMessage($"Invalid token: {ex.Message}", 401));
            }

            return response;
        }
    }
}
