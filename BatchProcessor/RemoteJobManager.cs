using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BatchProcessorAPI;

namespace BatchProcessor
{
    public class RemoteJobManager : IJobManager
    {
        TcpClient tcpClient;
        Action onDisconnect;
        System.Threading.Timer heartbeatTimer;
        int totalSlots;
        JobScheduler jobScheduler;

        public RemoteJobManager(TcpClient remote, Action onDisconnect)
        {
            this.tcpClient = remote;
            this.onDisconnect = onDisconnect;
            this.totalSlots = 0;

            string remoteHost;
            int remotePort;

            try
            {
                remoteHost = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();

                using (NetworkStream networkStream = tcpClient.GetStream())
                using (StreamReader reader = new StreamReader(networkStream))
                using (StreamWriter writer = new StreamWriter(networkStream))
                {                                        
                    remotePort = int.Parse(reader.ReadLine());
                    totalSlots = int.Parse(reader.ReadLine());
                }
            }
            catch
            {
                Dispose();
                return;
            }

            jobScheduler = new JobScheduler(remoteHost, remotePort);

            heartbeatTimer = new System.Threading.Timer(a => 
            {
                if (!tcpClient.Connected)
                    Dispose();

                try
                {
                    using (NetworkStream networkStream = tcpClient.GetStream())
                    using (StreamReader reader = new StreamReader(networkStream))
                    using (StreamWriter writer = new StreamWriter(networkStream))
                    {
                        writer.AutoFlush = true;
                        writer.WriteLine("PING");
                        string response = reader.ReadLine();
                        int slots = int.Parse(response);
                        if (slots != totalSlots)
                            Dispose();
                    }
                }
                catch
                {
                    Dispose();
                }   
            }, null, 0, 1000);
        }

        public void Dispose()
        {
            if (tcpClient.Connected)
                tcpClient.Close();
            heartbeatTimer.Dispose();
            onDisconnect();
        }

        public int GetTotalSlots()
        {
            return totalSlots;
        }

        public Task<JobResponse> ProcessJob(Job job)
        {
            return jobScheduler.Schedule(job);
        }
    }
}
