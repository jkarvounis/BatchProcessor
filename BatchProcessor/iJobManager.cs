using BatchProcessorAPI;
using System;
using System.Threading.Tasks;

namespace BatchProcessor
{
    public interface IJobManager : IDisposable
    {
        int GetTotalSlots();
        Task<JobResponse> ProcessJob(Job job);
    }
}
