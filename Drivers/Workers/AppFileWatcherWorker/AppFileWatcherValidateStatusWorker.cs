using Domain.Queues.AppFileDtos;
using Drivers.Services.AppFileWatcherService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Packages.Queues.Application.Services;

namespace Drivers.Workers.AppFileWatcherWorker
{
    public class AppFileWatcherValidateStatusWorker : BackgroundService
    {
        private readonly IQueueService<AppFileValidateStatusRequest> _queue;
        private readonly IServiceProvider _serviceProvider;

        public AppFileWatcherValidateStatusWorker(
            IQueueService<AppFileValidateStatusRequest> queue,
            IServiceProvider serviceProvider
        )
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var message in _queue.DequeueAsync(stoppingToken))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var watcherService = scope.ServiceProvider.GetRequiredService<IAppFileWatcherService>();

                    await Task.Run(() => watcherService.ValidateSync(message), stoppingToken);
                }
            }
        }
    }
}
