using BatchProcessorAPI;
using BatchProcessorServer.Models;
using BatchProcessorServer.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BatchProcessorServer.Data
{
    public static class DB
    {
        private static readonly SemaphoreSlim jobLocker = new SemaphoreSlim(1);        
        private static readonly Queue<JobItem> jobQueue = new Queue<JobItem>();

        private static readonly SemaphoreSlim workerLocker = new SemaphoreSlim(1);
        private static readonly Dictionary<Guid, WorkerInfo> workers = new Dictionary<Guid, WorkerInfo>();

        private static readonly SemaphoreSlim payloadLocker = new SemaphoreSlim(1);
        private static readonly Dictionary<Guid, DateTime> payloadAccess = new Dictionary<Guid, DateTime>();

        private static readonly SemaphoreSlim chartLocker = new SemaphoreSlim(1);
        private static readonly Dictionary<DateTimeOffset, ChartInfo> chartData = new Dictionary<DateTimeOffset, ChartInfo>();

        // Reset

        public static void Reset()
        {
            jobLocker.Wait();
            workerLocker.Wait();
            payloadLocker.Wait();

            foreach (var job in jobQueue)
            {
                job.response = new JobResponse
                {
                    ID = job.job.ID,
                    Name = job.job.Name,
                    Completed = false,
                    ConsoleError = "Job cancelled on server"
                };

                job.semaphore.Release();
            }
            jobQueue.Clear();

            foreach (var w in workers)
            {
                foreach (var job in w.Value.JobList.Values)
                {
                    job.response = new JobResponse
                    {
                        ID = job.job.ID,
                        Name = job.job.Name,
                        Completed = false,
                        ConsoleError = "Job cancelled on server"
                    };

                    job.semaphore.Release();
                }
                w.Value.JobList.Clear();
            }

            foreach(var p in payloadAccess)
            {
                string fileName = Path.Combine(Paths.TEMP_DIR, p.Key.ToString() + ".zip");
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            payloadAccess.Clear();

            payloadLocker.Release();
            workerLocker.Release();
            jobLocker.Release();
        }

        // Job Queue

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

        // Workers

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

        // Payloads

        public static async Task<Guid> CreatePayload(Stream payloadStream)
        {
            Guid newID = Guid.NewGuid();
            string tempFile = Path.Combine(Paths.TEMP_DIR, newID.ToString() + ".zip");

            FileStream tempFileStream = new FileStream(tempFile, FileMode.CreateNew);
            await payloadStream.CopyToAsync(tempFileStream);
            await tempFileStream.FlushAsync();
            tempFileStream.Close();

            await payloadLocker.WaitAsync();
            payloadAccess[newID] = DateTime.UtcNow;
            payloadLocker.Release();

            return newID;
        }

        public static async Task<string> GetPayloadFile(Guid id)
        {
            string fileName = Path.Combine(Paths.TEMP_DIR, id.ToString() + ".zip");

            if (!File.Exists(fileName))
                fileName = null;

            await payloadLocker.WaitAsync();
            if (payloadAccess.ContainsKey(id))
            {
                if (fileName != null)
                    payloadAccess[id] = DateTime.UtcNow;
                else
                    payloadAccess.Remove(id);
            }
            else
            {
                if (fileName != null)
                    payloadAccess.Remove(id);

                fileName = null;
            }

            payloadLocker.Release();

            return fileName;
        }

        public static async Task DeletePayload(Guid id)
        {
            string fileName = Path.Combine(Paths.TEMP_DIR, id.ToString() + ".zip");

            await payloadLocker.WaitAsync();
            if (payloadAccess.ContainsKey(id))
                payloadAccess.Remove(id);            
            payloadLocker.Release();

            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        public static int GetPayloadCount()
        {
            payloadLocker.Wait();
            int size = payloadAccess.Count;
            payloadLocker.Release();
            return size;
        }

        // Stats

        public static ChartModel GetChartData()
        {
            chartLocker.Wait();
            ChartModel output = new ChartModel()
            {
                QueueSize = new List<KeyValuePair<long, int>>(),
                PayloadCount = new List<KeyValuePair<long, int>>(),
                TotalCount = new List<KeyValuePair<long, int>>(),
                TotalCurrent = new List<KeyValuePair<long, int>>(),
                TotalWorkers = new List<KeyValuePair<long, int>>()
            };

            foreach (var element in chartData)
            {
                long time = element.Key.ToUnixTimeMilliseconds();
                output.QueueSize.Add(new KeyValuePair<long, int>(time, element.Value.QueueSize));
                output.PayloadCount.Add(new KeyValuePair<long, int>(time, element.Value.PayloadCount));
                output.TotalCount.Add(new KeyValuePair<long, int>(time, element.Value.TotalCount));
                output.TotalCurrent.Add(new KeyValuePair<long, int>(time, element.Value.TotalCurrent));
                output.TotalWorkers.Add(new KeyValuePair<long, int>(time, element.Value.TotalWorkers));
            }
            
            chartLocker.Release();
            return output;
        }

        // Update

        public static void Update(int heartbeatMs)
        {
            RecoverBadJobs(heartbeatMs);
            RemoveStalePayloads();
            UpdateChartData();
        }

        // Update Helper Methods
        private static void RecoverBadJobs(int heartbeatMs)
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

        private static void RemoveStalePayloads()
        {
            payloadLocker.Wait();

            DateTime oldData = DateTime.UtcNow - TimeSpan.FromHours(1);

            var oldKeys = payloadAccess.Where(pair => pair.Value < oldData).Select(pair => pair.Key).ToList();

            foreach (var id in oldKeys)
            {
                string fileName = Path.Combine(Paths.TEMP_DIR, id.ToString() + ".zip");
                
                if (payloadAccess.ContainsKey(id))
                    payloadAccess.Remove(id);

                if (File.Exists(fileName))
                    File.Delete(fileName);
            }

            payloadLocker.Release();
        }

        private static void UpdateChartData()
        {
            DateTimeOffset time = DateTimeOffset.UtcNow;
            var workers = GetWorkerInfo();
            ChartInfo data = new ChartInfo(
                QueueCount(),
                GetPayloadCount(),
                workers.Select(w => w.Count).Sum(),
                workers.Select(w => w.Current).Sum(),
                workers.Count);

            // Add new element and remove old keys
            DateTimeOffset oldData = time - TimeSpan.FromMinutes(30);
            chartLocker.Wait();
            chartData.Add(time, data);

            var oldKeys = chartData.Keys.Where(k => k < oldData).ToList();
            foreach (var k in oldKeys)
                chartData.Remove(k);

            chartLocker.Release();
        }
    }
}
