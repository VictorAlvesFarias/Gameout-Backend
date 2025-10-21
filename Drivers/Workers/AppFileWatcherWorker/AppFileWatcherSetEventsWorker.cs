using Domain.Entitites.ApplicationContextDb;
using Domain.Queues.AppFileDtos;
using Drivers.Services.AppFileWatcherService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Packages.Entity.Infraestructure.Repositories;
using Packages.Queues.Application.Services;

namespace Drivers.Workers.AppFileWatcherWorker
{
    public class AppFileWatcherSetEventsWorker : BackgroundService
    {
        private readonly IQueueService<AppFileSetEventsRequestMessage> _queue;
        private readonly IServiceProvider _serviceProvider;

        public AppFileWatcherSetEventsWorker(
            IQueueService<AppFileSetEventsRequestMessage> queue,
            IServiceProvider serviceProvider
        )
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var appFileRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<AppFile>>();
                var appFiles = appFileRepository.Get(true).Where(e => e.Observer || e.AutoValidateSync).ToList();

                await _queue.EnqueueAsync(new AppFileSetEventsRequestMessage
                {
                    AppFiles = appFiles
                });
            }

            await foreach (var message in _queue.DequeueAsync(stoppingToken))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var watcherService = scope.ServiceProvider.GetRequiredService<IAppFileWatcherService>();

                    await Task.Run(() => watcherService.SetWatchers(message), stoppingToken);
                }
            }
        }
    }
}
