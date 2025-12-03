using Application.Configuration;
using Application.Services.ApiKeyService;
using Application.Services.ApplicationLogService;
using Application.Services.AppFileService;
using Application.Services.Identity;
using Application.Services.IdentityService;
using Domain.Entitites.ApplicationContextDb;
using Infrastructure.Context;
using Infrastructure.Factories;
using Infrastructure.Mediators;
using Microsoft.AspNetCore.Identity;
using Web.Api.Toolkit.Entity.Infraestructure.Factories;
using Web.Api.Toolkit.Entity.Infraestructure.Mediators;
using Web.Api.Toolkit.Entity.Infraestructure.Repositories;
using Web.Api.Toolkit.Identity.Application.Extensions;
using Web.Api.Toolkit.Ws.Application.Workers;

namespace ASP.NET_Core_Template.Ioc
{
    public static class NativeInjectorConfig
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ApplicationContext>();
            services.AddSingleton<WebSocketWorker>();

            services.AddHostedService(provider => provider.GetRequiredService<WebSocketWorker>());

            services.AddDefaultIdentity<ApplicationUser, IdentityRole, ApplicationDbContext>();

            services.AddScoped<IDatabaseContextFactory, DbContextFactory>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped(typeof(IDatabaseContextMediator<>), typeof(DatabaseContextMediator<>));
            services.AddScoped<IAppFileService, AppFileService>();
            services.AddScoped<IApplicationLogService, ApplicationLogService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();

            // Configure DriverApiKeyOptions
            services.Configure<DriverApiKeyOptions>(configuration.GetSection(DriverApiKeyOptions.SectionName));
        }
    }
}
