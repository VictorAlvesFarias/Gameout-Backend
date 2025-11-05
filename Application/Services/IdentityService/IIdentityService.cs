using Web.Api.Toolkit.Helpers.Application.Dtos;
using Web.Api.Toolkit.Identity.Application.Dtos;
using Web.Api.Toolkit.Identity.Application.Services;
using Web.Api.Toolkit.Identity.Domain.Entities;

namespace Application.Services.Identity
{
    public interface IIdentityService : IIdentityUtilsService<ApplicationUser>
    {
        Task<DefaultResponse> AddUser(CreateUserRequest userData);
        Task<DefaultResponse> PutUser(PutUserRequest userData);
    }
}
