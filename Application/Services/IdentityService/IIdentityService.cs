using Application.Dtos.User;
using Web.Api.Toolkit.Helpers.Application.Dtos;
using Web.Api.Toolkit.Identity.Application.Dtos;

namespace Application.Services.Identity
{
    public interface IIdentityService
    {
        Task<BaseResponse<LoginUserResponse>> LoginAsync(LoginUserRequest loginData);
        Task<DefaultResponse> AddUser(CreateUserRequest userData);
        Task<DefaultResponse> DeleteSignedUser(LoginUserRequest email);
        Task<DefaultResponse> PutUser(PutUserRequest userData);
        Task<DefaultResponse> ValidateUsernameAsync(string email);
        Task<DefaultResponse> ValidateEmailAsync(string email);
        Task<BaseResponse<GetUserResponseDto>> GetCurrentUserAsync();
        Task<DefaultResponse> ChangePasswordAsync(ChangePasswordRequest changePasswordData);
    }
}