using System.Threading;
using System.Threading.Tasks;
using BatchProcessorAPI;

namespace BatchProcessor
{
    public class LocalJobManager : IJobManager
    {
        SemaphoreSlim concurrencyLimit;
        readonly int totalSlots;

        public LocalJobManager(int localSlots)
        {
            concurrencyLimit = new SemaphoreSlim(localSlots);
            totalSlots = localSlots;
        }

        public int GetTotalSlots()
        {
            return totalSlots;
        }

        public Task<JobResponse> ProcessJob(Job job)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await concurrencyLimit.WaitAsync();
                    return JobExecutor.Execute(job);
                }
                finally
                {
                    concurrencyLimit.Release();
                }
            });
        }

        public void Dispose()
        {
            concurrencyLimit.Dispose();
        }
    }
}
