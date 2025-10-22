using Application.Services.AppFileLogService;
using Application.Services.AppFileService;
using Domain.Entitites;
using Infrastructure.Context;
using Infrastructure.Factories;
using Infrastructure.Mediators;
using Microsoft.AspNetCore.Identity;
using Packages.Entity.Infraestructure.Factories;
using Packages.Entity.Infraestructure.Mediators;
using Packages.Entity.Infraestructure.Repositories;
using Packages.Identity.Application.Extensions;
using Packages.Identity.Application.Services;
using Packages.Queues.Application.Services;
using Packages.Ws.Application.Workers;

namespace ASP.NET_Core_Template.Ioc
{
    public static class NativeInjectorConfig
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ApplicationContext>();
            services.AddSingleton(typeof(IQueueService<>), typeof(QueueService<>));

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
