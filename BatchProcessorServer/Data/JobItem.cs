using BatchProcessorAPI;
using System;
using System.Threading;

namespace BatchProcessorServer.Data
{
    public class JobItem
    {
        public Job job;
        public DateTime? heartbeat;
        public Guid? workerID;
        public SemaphoreSlim semaphore;
        public JobResponse response;

        public JobItem(Job j)
        {
            job = j;
            heartbeat = null;
            workerID = null;
            semaphore = new SemaphoreSlim(0);
            response = null;
        }
    }
}
