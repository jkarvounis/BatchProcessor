using BatchProcessorServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BatchProcessorServer.Data
{
    public static class DB
    {
        private static SemaphoreSlim jobLocker = new SemaphoreSlim(1);        
        private static Queue<JobItem> jobQueue = new Queue<JobItem>();

        private static SemaphoreSlim workerLocker = new SemaphoreSlim(1);
        private static Dictionary<Guid, WorkerInfo> workers = new Dictionary<Guid, WorkerInfo>();

        public static int QueueCount()
        {
            jobLocker.Wait();
            int count = jobQueue.Count;
            jobLocker.Release();
            return count;
        }

        public static async Task QueueJobItemAsync(JobItem jobItem)
        {
            await jobLocker.WaitAsync();
            jobQueue.Enqueue(jobItem);
            jobLocker.Release();
        }

        public static async Task<JobItem> DeqeueueJobItemAsync(Guid workerID)
        {
            await jobLocker.WaitAsync();
            if (jobQueue.Count == 0)
            {
                jobLocker.Release();
                return null;
            }

            JobItem job = jobQueue.Dequeue();
            jobLocker.Release();

            job.heartbeat = DateTime.UtcNow;
            job.workerID = workerID;

            await workerLocker.WaitAsync();
            if (!workers.ContainsKey(workerID))
                workers.Add(workerID, new WorkerInfo(workerID));
            workers[workerID].AddJobItem(job);
            workerLocker.Release();

            return job;
        }

        public static async Task RegisterWorkerAsync(Guid workerID, int slotCount, string name)
        {
            await workerLocker.WaitAsync();

            if (!workers.ContainsKey(workerID))
                workers.Add(workerID, new WorkerInfo(workerID));

            workers[workerID].SetRegistrationInfo(slotCount, name);

            workerLocker.Release();
        }

        public static List<WorkerModel> GetWorkerInfo()
        {
            workerLocker.Wait();
            List<WorkerModel> count = new List<WorkerModel>();
            foreach (var pair in workers)
                count.Add(new WorkerModel()
                {
                    ID = pair.Value.ID,
                    Name = pair.Value.Name,
                    Count = pair.Value.Slots,
                    Current = pair.Value.JobList.Count
                });
            workerLocker.Release();
            return count;
        }

        public static async Task<bool> StoreHeartbeat(Guid workerID, Guid jobID)
        {
            await workerLocker.WaitAsync();
            if (!workers.ContainsKey(workerID))
            {
                workerLocker.Release();
                return false;
            }

            if (!workers[workerID].JobList.ContainsKey(jobID))
            {
                workerLocker.Release();
                return false;
            }

            workers[workerID].JobList[jobID].heartbeat = DateTime.UtcNow;
            workerLocker.Release();

            return true;
        }

        public static async Task<JobItem> RemoveJobForResponse(Guid workerID, Guid jobID)
        {
            await workerLocker.WaitAsync();
            if (!workers.ContainsKey(workerID))
            {
                workerLocker.Release();
                return null;
            }

            if (!workers[workerID].JobList.ContainsKey(jobID))
            {
                workerLocker.Release();
                return null;
            }

            JobItem jobItem = workers[workerID].JobList[jobID];
            workers[workerID].JobList.Remove(jobID);
            workerLocker.Release();

            return jobItem;
        }

        public static void RecoverBadJobs(int heartbeatMs)
        {
            workerLocker.Wait();
            DateTime threshold = DateTime.UtcNow - TimeSpan.FromMilliseconds(2 * heartbeatMs);
            foreach (var workerID in workers.Keys.ToList())
            {
                bool removeWorker = workers[workerID].RegistrationTime < threshold;
                var list = workers[workerID].JobList.Values.ToList();
                foreach (var job in list)
                {
                    if (removeWorker || job.heartbeat.GetValueOrDefault(DateTime.MinValue) < threshold)
                    {
                        workers[workerID].JobList.Remove(job.job.ID);
                        job.workerID = null;
                        job.heartbeat = null;

                        jobLocker.Wait();
                        jobQueue.Enqueue(job);
                        jobLocker.Release();
                    }
                }

                if (removeWorker)
                    workers.Remove(workerID);
            }
            workerLocker.Release();
        }
    }
}
