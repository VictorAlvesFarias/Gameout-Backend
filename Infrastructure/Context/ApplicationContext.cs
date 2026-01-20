using Domain.Entitites.ApplicationContext;
using Domain.Queues.AppFileDtos;
using Web.Api.Toolkit.Queues.Application.Services;

namespace Infrastructure.Context
{
    public class ApplicationContext
    {
        public List<AppFileWatcher> AppFileWatchers { get; set; }
        public IQueueService<AppFileProcessingQueueItem> AppFileProcessingQueue { get; set; }

        public ApplicationContext(IQueueService<AppFileProcessingQueueItem> appFileProcessingQueue)
        {
            AppFileWatchers = new List<AppFileWatcher>();
            AppFileProcessingQueue = appFileProcessingQueue;
        }
    }
}
