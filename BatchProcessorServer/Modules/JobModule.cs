using BatchProcessorAPI;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BatchProcessorServer.Modules
{
    public class JobModule : NancyModule
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

        private static SemaphoreSlim locker = new SemaphoreSlim(1);
        private static Queue<JobItem> jobQueue = new Queue<JobItem>();
        private static Dictionary<Guid, Dictionary<Guid, JobItem>> jobList = new Dictionary<Guid, Dictionary<Guid, JobItem>>();
        
        public static int QueueCount()
        {
            locker.Wait();
            int count = jobQueue.Count;
            locker.Release();
            return count;
        }

        public static Dictionary<Guid, int> WorkerCounts()
        {
            locker.Wait();
            Dictionary<Guid, int> count = new Dictionary<Guid, int>();
            foreach (var pair in jobList)
                count.Add(pair.Key, pair.Value.Count);
            locker.Release();
            return count;
        }

        public static void RecoverBadJobs()
        {
            locker.Wait();
            DateTime threshold = DateTime.UtcNow - TimeSpan.FromSeconds(10);
            foreach(var workerID in jobList.Keys.ToList())
            {
                var list = jobList[workerID].Values.ToList();
                foreach(var job in list)
                {
                    if (job.heartbeat.GetValueOrDefault(DateTime.MinValue) < threshold)
                    {
                        jobList[workerID].Remove(job.job.ID);
                        job.workerID = null;
                        job.heartbeat = null;
                        jobQueue.Enqueue(job);
                    }
                }
            }
            locker.Release();
        }

        public JobModule() : base("/job")
        {
            Post("/", async _ =>
            {
                Job job = this.Bind<Job>();
                JobItem jobItem = new JobItem(job);
                await locker.WaitAsync();
                jobQueue.Enqueue(jobItem);
                locker.Release();

                await jobItem.semaphore.WaitAsync();
                return jobItem.response;
            });

            Get("/queue/{workerID}", async parameters =>
            {
                await locker.WaitAsync();
                if (jobQueue.Count == 0)
                {
                    locker.Release();
                    return HttpStatusCode.NoContent;
                }

                JobItem job = jobQueue.Dequeue();
                job.heartbeat = DateTime.UtcNow;
                job.workerID = parameters.workerID;
                if (!jobList.ContainsKey(parameters.workerID))
                    jobList.Add(parameters.workerID, new Dictionary<Guid, JobItem>());
                jobList[parameters.workerID].Add(job.job.ID, job);

                locker.Release();

                return job.job;
            });

            Put("/{jobID}/{workerID}/heatbeat", async parameters =>
            {
                await locker.WaitAsync();
                if (!jobList.ContainsKey(parameters.workerID))
                {
                    locker.Release();
                    return HttpStatusCode.NotFound;
                }

                if (!jobList[parameters.workerID].ContainsKey(parameters.jobID))
                {
                    locker.Release();
                    return HttpStatusCode.NotFound;
                }

                jobList[parameters.workerID][parameters.jobID].heartbeat = DateTime.UtcNow;
                locker.Release();

                return HttpStatusCode.OK;
            });

            Delete("/{jobID}/{workerID}", async parameters =>
            {
                await locker.WaitAsync();
                if (!jobList.ContainsKey(parameters.workerID))
                {
                    locker.Release();
                    return HttpStatusCode.NotFound;
                }

                if (!jobList[parameters.workerID].ContainsKey(parameters.jobID))
                {
                    locker.Release();
                    return HttpStatusCode.NotFound;
                }

                JobItem job = jobList[parameters.workerID][parameters.jobID];
                jobList[parameters.workerID].Remove(parameters.jobID);
                locker.Release();

                job.response = this.Bind<JobResponse>();
                job.semaphore.Release();

                return HttpStatusCode.OK;
            });
        }
    }
}
