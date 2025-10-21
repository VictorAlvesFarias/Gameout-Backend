using Domain.Queues.AppFileDtos;
using Drivers.Services.AppFileWatcherService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Packages.Queues.Application.Services;

namespace Drivers.Workers.AppFileWatcherWorker
{
    public class AppFileWatcherStatusCheckWorker : BackgroundService
    {
        private readonly IQueueService<AppFileStatusCheckRequestMessage> _requestQueue;
        private readonly IServiceProvider _serviceProvider;

        public AppFileWatcherStatusCheckWorker(
            IQueueService<AppFileStatusCheckRequestMessage> requestQueue,
            IQueueService<AppFileStatusCheckResponseMessage> responseQueue,
            IServiceProvider serviceProvider
        )
        {
            _requestQueue = requestQueue;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var job in _requestQueue.DequeueAsync(stoppingToken))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var appFileWatcherService = scope.ServiceProvider.GetRequiredService<IAppFileWatcherService>();

                    await Task.Run(() => appFileWatcherService.IsProcessing(job), stoppingToken);
                }
            }
        }
    }
}
