using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Web.Api.Toolkit.Helpers.Application.Dtos;
using Web.Api.Toolkit.Identity.Application.Configuration;
using Web.Api.Toolkit.Identity.Application.Dtos;
using Web.Api.Toolkit.Identity.Application.Services;
using Web.Api.Toolkit.Identity.Domain.Entities;

namespace Application.Services.Identity
{
    public class IdentityService : IdentityUtilsService<ApplicationUser>, IIdentityService, IIdentityUtilsService<ApplicationUser>
    {
        private readonly ApplicationContext _context;
        private readonly SignInManager<ApplicationUser> _singInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtOptions _jwtOptions;
        private readonly string _userId;

        public IdentityService(
            ApplicationContext context, 
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager, 
            IOptions<JwtOptions> jwtOptions
        ) : base(
            signInManager,
            userManager,
            jwtOptions
        )
        {
            _context = context;
            _singInManager = signInManager;
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<DefaultResponse> PutUser(PutUserRequest userData)
        {
            var user = await _userManager.FindByIdAsync(_userId);

            var response = new DefaultResponse();

            if (user != null)
            {
                user.Name = userData.Name ==  null ? user.Name : userData.Name;
                user.Email = userData.Email == null ? user.Email : userData.Email;
                user.UserName = userData.Username == null ? user.UserName : userData.Username;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    response.Success = true;
                }

                else 
                {
                    response.AddErrors(result.Errors.ToList().ConvertAll(item=> new ErrorMessage(item.Description)));

                    response.Success = false;
                }


                return response;
            }

            else
            {
                response.AddError(new ErrorMessage("Faça login novamente e tente mais tarde."));

                response.Success = false;

                return response;
            }
        }
        public async Task<DefaultResponse> AddUser(CreateUserRequest userData)
        {
            var validateEmail = await ValidateEmailAsync(userData.Email);
            
            if (!validateEmail.Success)
            {
                return validateEmail;
            }

            var user = new ApplicationUser()
            {
                UserName = userData.Username,
                Email = userData.Email,
                CreateDate = DateTime.UtcNow,
                EmailConfirmed = false,
                Name = userData.Name,
            };
            var createdUser = await _userManager.CreateAsync(user, userData.Password);
            var defaultResponse = new DefaultResponse(createdUser.Succeeded);
            
            if (!defaultResponse.Success)
            {
                defaultResponse.AddErrors(createdUser.Errors.ToList().ConvertAll(item => new ErrorMessage(item.Description)));
            }

            return defaultResponse;
        }
    }
}