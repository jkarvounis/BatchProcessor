using BatchProcessorAPI.RestUtil;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BatchProcessorAPI
{
    /// <summary>
    /// Class that sends jobs to the server and waits for responses
    /// </summary>
    public class JobScheduler
    {
        readonly IRestClient client;
        Guid? payloadID;

        /// <summary>
        /// Constructor for the scheduler.  Requires remote server to be running with a valid hostname and TCP port
        /// </summary>
        /// <param name="serverHostname">Hostname of the server</param>
        /// <param name="tcpPort">TCP port of the server</param>
        public JobScheduler(string serverHostname, int tcpPort)
        {
            client = new RestClient($"http://{serverHostname}:{tcpPort}")
                .UseSerializer(() => new JsonNetSerializer());
            payloadID = null;
        }

        /// <summary>
        /// Uploads a payload for jobs
        /// </summary>
        /// <param name="path">Path to a .zip of the current payload</param>
        /// <returns>True if successful</returns>
        public bool UploadPayload(string path)
        {
            Task<bool> task = Task.Run(async () => await UploadPayloadAsync(path));            
            return task.Result;
        }

        /// <summary>
        /// Uploads a payload for jobs 
        /// </summary>
        /// <param name="path">Path to a .zip of the current payload</param>
        /// <returns>True if successful</returns>
        public async Task<bool> UploadPayloadAsync(string path)
        {
            try
            {
                var request = new RestRequest("payload");
                request.AddFile("File", path);

                var response = await client.PostAsync<Guid>(request);

                if (response != null)
                {
                    payloadID = response;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Uploads a payload for jobs 
        /// </summary>
        /// <param name="payload">Binary content of a .zip of the current payload</param>
        /// <returns>True if successful</returns>
        public bool UploadPayload(byte[] payload)
        {
            Task<bool> task = Task.Run(async () => await UploadPayloadAsync(payload));
            return task.Result;
        }

        /// <summary>
        /// Uploads a payload for jobs 
        /// </summary>
        /// <param name="payload">Binary content of a .zip of the current payload</param>
        /// <returns>True if successful</returns>
        public async Task<bool> UploadPayloadAsync(byte[] payload)
        {
            try
            {
                var request = new RestRequest("payload");
                request.AddFileBytes("File", payload, "payload.zip");

                var response = await client.PostAsync<Guid>(request);

                if (response != null)
                {
                    payloadID = response;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Async function to schedule a job on the server 
        /// </summary>
        /// <param name="job">Job to execute remotely</param>
        /// <returns>Job response from server</returns>
        public async Task<JobResponse> ScheduleAsync(Job job)
        {            
            try
            {
                job.PayloadID = payloadID;

                var request = new RestRequest("job", DataFormat.Json);
                request.AddJsonBody(job);

                var response = await client.PostAsync<JobResponse>(request);
                
                if (response != null)
                    return response;

                return OnError(job, "No response from server");                
            }
            catch (Exception ex)
            {
                return OnError(job, "Exception in JobScheduler: " + ex.ToString());
            }
        }

        /// <summary>
        /// Executes all the jobs on the server, waits for all the jobs to complete, calls delegates on complete or fail per job
        /// </summary>
        /// <param name="jobs">List of jobs to execute</param>
        /// <param name="onResponse">Action on each job response as they complete</param>
        /// <returns>Returns the list of job results after they all complete</returns>
        public List<JobResponse> ScheduleAll(List<Job> jobs, Action<JobResponse> onResponse)
        {
            List<Task<JobResponse>> taskList = new List<Task<JobResponse>>();

            foreach (var job in jobs)
            {
                var j = job;
                taskList.Add(Task.Run(async () =>
                {
                    JobResponse result = await ScheduleAsync(j);
                    onResponse(result);
                    return result;
                }));
            }

            Task.WaitAll(taskList.ToArray());            

            return taskList.Select(x=>x.Result).ToList();
        }

        private static JobResponse OnError(Job job, string message)
        {
            return new JobResponse()
            {
                Name = job.Name,
                Completed = false,
                ReturnFile = null,
                ConsoleOutput = null,
                ConsoleError = message
            };
        }
    }
}
