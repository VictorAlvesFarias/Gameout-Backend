using Domain.Queues.AppFileDtos;
using Drivers.Services.AppFileWatcherService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Packages.Queues.Application.Services;

namespace Drivers.Workers.AppFileWatcherWorker
{
    public class AppFileWatcherUpdateWorker : BackgroundService
    {
        private readonly IQueueService<AppFileUpdateRequestMessage> _queue;
        private readonly IServiceProvider _serviceProvider;

        public AppFileWatcherUpdateWorker(
            IQueueService<AppFileUpdateRequestMessage> queue,
            IServiceProvider serviceProvider
        )
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var handle in _queue.DequeueWithHandleEnumerable(stoppingToken))
            {
                using var scope = _serviceProvider.CreateScope();
                var watcherService = scope.ServiceProvider.GetRequiredService<IAppFileWatcherService>();

                await using (handle) // item só é removido do buffer quando o bloco terminar
                {
                    await Task.Run(() => watcherService.SingleSync(handle.Item));
                }
            }

        }
    }
}
