using BatchProcessorAPI.StreamUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcessorAPI
{
    /// <summary>
    /// Class that sends jobs to the server and waits for responses
    /// </summary>
    public class JobScheduler
    {
        string hostname;
        int port;

        /// <summary>
        /// Constructor for the scheduler.  Requires remote server to be running with a valid hostname and TCP port
        /// </summary>
        /// <param name="serverHostname">Hostname of the server</param>
        /// <param name="tcpPort">TCP port of the server</param>
        public JobScheduler(string serverHostname, int tcpPort)
        {
            this.hostname = serverHostname;
            this.port = tcpPort;
        }

        /// <summary>
        /// Async function to schedule a job on the server 
        /// </summary>
        /// <param name="job">Job to execute remotely</param>
        /// <returns>Job response from server</returns>
        public async Task<JobResponse> Schedule(Job job)
        {                        
            try
            {
                using (TcpClient tcpClient = new TcpClient(hostname, port))
                using (NetworkStream networkStream = tcpClient.GetStream())
                using (StreamReader reader = new StreamReader(networkStream))
                {                    
                    await SerializeJob(networkStream, job);
                    
                    string response = await reader.ReadLineAsync();
                    if (response != null)
                        return DeserializeResponse(response);
                    return OnError(job, "No response from server");
                }
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
                    JobResponse result = await Schedule(j);
                    onResponse(result);
                    return result;
                }));
            }

            Task.WaitAll(taskList.ToArray());            

            return taskList.Select(x=>x.Result).ToList();
        }

        private static JobResponse DeserializeResponse(string base64response)
        {
            byte[] responseData = Convert.FromBase64String(base64response);
            MemoryStream ms = new MemoryStream(responseData);
            using (BsonDataReader bsonReader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<JobResponse>(bsonReader);
            }
        }

        private static async Task SerializeJob(Stream stream, Job job)
        {
            using (NotClosingCryptoStream crypto = new NotClosingCryptoStream(stream, new ToBase64Transform(), CryptoStreamMode.Write))
            {
                using (BsonDataWriter writer = new BsonDataWriter(crypto))
                {
                    writer.CloseOutput = false;

                    JsonSerializer serializer = new JsonSerializer();                    
                    serializer.Serialize(writer, job);                    
                    await writer.FlushAsync();
                }                    
            }
            
            using (NotClosingStreamWriter writer = new NotClosingStreamWriter(stream))
            {
                await writer.WriteLineAsync();
                await writer.FlushAsync();
            }
        }

        private static JobResponse OnError(Job job, string message)
        {
            return new JobResponse()
            {
                ID = job.ID,
                Name = job.Name,
                Completed = false,
                ReturnFile = null,
                ConsoleOutput = null,
                ConsoleError = message
            };
        }
    }
}
