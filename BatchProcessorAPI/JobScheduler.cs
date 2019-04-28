using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
            string data = SerializeJob(job);
            
            try
            {
                using (TcpClient tcpClient = new TcpClient(hostname, port))
                using (NetworkStream networkStream = tcpClient.GetStream())
                using (StreamReader reader = new StreamReader(networkStream))
                using (StreamWriter writer = new StreamWriter(networkStream))
                {                    
                    writer.AutoFlush = true;
                    await writer.WriteLineAsync(data);
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
        /// <param name="onResponse">Action on job response</param>
        public void ScheduleAll(List<Job> jobs, Action<JobResponse> onResponse)
        {
            var tasks = jobs.Select(async j =>
            {
                var r = await Schedule(j);
                onResponse(r);
            }).ToArray();                

            Task.WaitAll(tasks);
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

        private static string SerializeJob(Job job)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BsonDataWriter writer = new BsonDataWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, job);
                }
                return Convert.ToBase64String(ms.ToArray());
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
