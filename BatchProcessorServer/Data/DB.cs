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
        private static SemaphoreSlim locker = new SemaphoreSlim(1);        

        private static Queue<JobItem> jobQueue = new Queue<JobItem>();
        private static Dictionary<Guid, WorkerInfo> workers = new Dictionary<Guid, WorkerInfo>();

        public static int QueueCount()
        {
            locker.Wait();
            int count = jobQueue.Count;
            locker.Release();
            return count;
        }

        public static async Task QueueJobItemAsync(JobItem jobItem)
        {
            await locker.WaitAsync();
            jobQueue.Enqueue(jobItem);
            locker.Release();
        }

        public static async Task<JobItem> DeqeueueJobItemAsync(Guid workerID)
        {
            await locker.WaitAsync();
            if (jobQueue.Count == 0)
            {
                locker.Release();
                return null;
            }

            JobItem job = jobQueue.Dequeue();
            job.heartbeat = DateTime.UtcNow;
            job.workerID = workerID;
            if (!workers.ContainsKey(workerID))
                workers.Add(workerID, new WorkerInfo(workerID));
            workers[workerID].AddJobItem(job);

            locker.Release();
            return job;
        }

        public static async Task RegisterWorkerAsync(Guid workerID, int slotCount, string name)
        {
            await locker.WaitAsync();

            if (!workers.ContainsKey(workerID))
                workers.Add(workerID, new WorkerInfo(workerID));

            workers[workerID].SetRegistrationInfo(slotCount, name);

            locker.Release();
        }

        public static List<WorkerModel> GetWorkerInfo()
        {
            locker.Wait();
            List<WorkerModel> count = new List<WorkerModel>();
            foreach (var pair in workers)
                count.Add(new WorkerModel()
                {
                    ID = pair.Value.ID,
                    Name = pair.Value.Name,
                    Count = pair.Value.Slots,
                    Current = pair.Value.JobList.Count
                });
            locker.Release();
            return count;
        }

        public static async Task<bool> StoreHeartbeat(Guid workerID, Guid jobID)
        {
            await locker.WaitAsync();
            if (!workers.ContainsKey(workerID))
            {
                locker.Release();
                return false;
            }

            if (!workers[workerID].JobList.ContainsKey(jobID))
            {
                locker.Release();
                return false;
            }

            workers[workerID].JobList[jobID].heartbeat = DateTime.UtcNow;
            locker.Release();

            return true;
        }

        public static async Task<JobItem> RemoveJobForResponse(Guid workerID, Guid jobID)
        {
            await locker.WaitAsync();
            if (!workers.ContainsKey(workerID))
            {
                locker.Release();
                return null;
            }

            if (!workers[workerID].JobList.ContainsKey(jobID))
            {
                locker.Release();
                return null;
            }

            JobItem jobItem = workers[workerID].JobList[jobID];
            workers[workerID].JobList.Remove(jobID);
            locker.Release();

            return jobItem;
        }

        public static void RecoverBadJobs(int heartbeatMs)
        {
            locker.Wait();
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
                        jobQueue.Enqueue(job);
                    }
                }

                if (removeWorker)
                    workers.Remove(workerID);
            }
            locker.Release();
        }
    }
}
