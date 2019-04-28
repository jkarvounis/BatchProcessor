using BatchProcessorAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BatchProcessor
{
    public class JobListener : IDisposable
    {
        TcpListener tcpListener;
        Thread listenerThread;
        SemaphoreSlim concurrencyLimit;
        IJobManager jobManager;
        bool isRunning;
        int maxThreads;

        public JobListener(int tcpPort, IJobManager jobManager)
        {
            this.jobManager = jobManager;
            int max = Math.Max(10000, 2 * jobManager.GetTotalSlots());
            maxThreads = jobManager.GetTotalSlots();
            this.concurrencyLimit = new SemaphoreSlim(maxThreads, max);

            isRunning = true;
            tcpListener = new TcpListener(IPAddress.Any, tcpPort);
            listenerThread = new Thread(new ThreadStart(() => 
            {
                Console.WriteLine("JobListener listenerThread started");
                try
                {
                    tcpListener.Start();
                    while (isRunning)
                    {
                        TcpClient remote = tcpListener.AcceptTcpClient();
                        Thread remoteHandler = new Thread(new ParameterizedThreadStart(HandleRemoteClientAsync));
                        remoteHandler.Start(remote);
                    }
                }
                catch (Exception)
                { }
                Console.WriteLine("JobListener listenerThread exited");
            }));
            listenerThread.Start();
        }

        ~JobListener()
        {
            Dispose();
        }

        private async void HandleRemoteClientAsync(object r)
        {
            TcpClient tcpClient = (TcpClient)r;
            string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine("Received connection request from " + clientEndPoint);
            concurrencyLimit.Wait();
            try
            {
                using (NetworkStream networkStream = tcpClient.GetStream())
                using (StreamReader reader = new StreamReader(networkStream))
                using (StreamWriter writer = new StreamWriter(networkStream))
                {
                    writer.AutoFlush = true;

                    string request = reader.ReadLine();
                    if (request != null)
                    {                                               
                        Job jobRequest = DeserializeRequest(request);
                        JobResponse responseJob = await jobManager.ProcessJob(jobRequest);
                        string response = SerializeResponse(responseJob);                        
                        writer.WriteLine(response);
                        Console.WriteLine("Completed connection request from " + clientEndPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in HandleRemoteClient: " + ex.ToString());                
            }
            finally
            {
                concurrencyLimit.Release();
                if (tcpClient.Connected)
                    tcpClient.Close();                
            }
            UpdateMaxThreads();
        }

        private void UpdateMaxThreads()
        {
            lock (jobManager)
            {
                int max = jobManager.GetTotalSlots();
                if (max > maxThreads)
                {
                    concurrencyLimit.Release(max - maxThreads);
                }
                else if (max < maxThreads)
                {
                    for (int i = max; i < maxThreads; i++)
                        concurrencyLimit.Wait();
                }
                maxThreads = max;
            }
        }

        public bool Connected { get { return isRunning && tcpListener != null; } }

        public void Dispose()
        {
            isRunning = false;
            if (listenerThread != null)
            {
                tcpListener.Stop();
                listenerThread = null;
            }
        }

        private static Job DeserializeRequest(string request)
        {
            byte[] requestData = Convert.FromBase64String(request);
            MemoryStream ms = new MemoryStream(requestData);
            using (BsonDataReader bsonReader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<Job>(bsonReader);
            }
        }

        private static string SerializeResponse(JobResponse responseJob)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BsonDataWriter bw = new BsonDataWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(bw, responseJob);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}
