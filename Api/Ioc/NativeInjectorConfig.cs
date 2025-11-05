using Application.Services.AppFileLogService;
using Application.Services.AppFileService;
using Domain.Entitites;
using Infrastructure.Context;
using Infrastructure.Factories;
using Infrastructure.Mediators;
using Microsoft.AspNetCore.Identity;
using Web.Api.Toolkit.Entity.Infraestructure.Factories;
using Web.Api.Toolkit.Entity.Infraestructure.Mediators;
using Web.Api.Toolkit.Entity.Infraestructure.Repositories;
using Web.Api.Toolkit.Identity.Application.Extensions;
using Web.Api.Toolkit.Identity.Application.Services;
using Web.Api.Toolkit.Identity.Domain.Entities;
using Web.Api.Toolkit.Ws.Application.Workers;

namespace ASP.NET_Core_Template.Ioc
{
    public static class NativeInjectorConfig
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ApplicationContext>();

            services.AddScoped<IDatabaseContextFactory, DbContextFactory>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped(typeof(IDatabaseContextMediator<>), typeof(DatabaseContextMediator<>));
            services.AddScoped<IAppFileService, AppFileService>();
            services.AddScoped<IAppFileLogService, AppFileLogService>();

            services.AddDefaultIdentity<BaseEntityIdentity, IdentityRole, ApplicationDbContext>();
            services.AddScoped<IIdentityService, IdentityService>();

            services.AddSingleton<WebSocketWorker>();
            services.AddHostedService(provider => provider.GetRequiredService<WebSocketWorker>());
        }
    }
}
