﻿using BatchProcessorAPI;
using RestSharp;
using System;
using System.Threading;

namespace BatchProcessor.Jobs
{
    public class JobModule : IDisposable
    {
        private static readonly int THREAD_SLEEP_MS = 5000;

        IRestClient client;
        Guid workerID;
        int threadID;

        SemaphoreSlim locker = new SemaphoreSlim(1);
        Guid? _currentJobID = null;
        Guid? currentJobID
        {
            get
            {
                locker.Wait();
                Guid? ret = _currentJobID;
                locker.Release();
                return ret;
            }
            set
            {
                locker.Wait();
                _currentJobID = value;
                locker.Release();
            }
        }

        Thread thread;
        bool isRunning;

        public JobModule(IRestClient client, Guid workerID, int threadID)
        {
            this.client = client;
            this.workerID = workerID;
            this.threadID = threadID;

            isRunning = true;
            thread = new Thread(new ThreadStart(mainLoop));
            thread.Start();
        }

        private void mainLoop()
        {
            while (isRunning)
            {
                Job job = FetchJob();
                if (job == null)
                {
                    Thread.Sleep(THREAD_SLEEP_MS);
                    continue;
                }

                currentJobID = job.ID;

                Console.WriteLine($"Working on Job: {job.ID}");

                string payloadDir = null;
                if (job.PayloadID != null)
                {
                    payloadDir = BatchProcessor.Util.PayloadUtil.GetPayload(client, job.PayloadID.Value, threadID);
                    if (payloadDir == null)
                    {
                        Console.WriteLine("FAILED TO GET PAYLOAD");
                        currentJobID = null;                        
                        Thread.Sleep(THREAD_SLEEP_MS);
                        continue;
                    }
                }

                JobResponse response = JobExecutor.Execute(job, payloadDir);
                SendResponse(response);

                if (job.PayloadID != null)
                {
                    BatchProcessor.Util.PayloadUtil.ReleasePayload(job.PayloadID.Value, threadID);
                }

                currentJobID = null;
            }
        }

        public bool HasJob()
        {
            return currentJobID != null;
        }

        public bool SendHeartbeat()
        {
            Guid? id = currentJobID;

            if (id == null)
                return false;

            try
            {
                var request = new RestRequest($"job/{id}/{workerID}/heartbeat");
                var response = client.Put(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    return true;
            }
            catch
            {
            }

            return false;
        }

        public void Dispose()
        {
            isRunning = false;
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        private Job FetchJob()
        {
            try
            {
                var request = new RestRequest($"job/queue/{workerID}");
                var response = client.Get<Job>(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    return response.Data;                
            }
            catch
            {                
            }

            return null;
        }

        private bool SendResponse(JobResponse results)
        {
            try
            {
                var request = new RestRequest($"job/{results.ID}/{workerID}");
                request.AddJsonBody(results);
                var response = client.Delete(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    return true;
            }
            catch
            {
            }

            return false;
        }
    }
}
