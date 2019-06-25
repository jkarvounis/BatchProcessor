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
        NetworkStream networkStream;
        StreamReader reader;
        StreamWriter writer;

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

                networkStream = tcpClient.GetStream();
                reader = new StreamReader(networkStream);
                writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;

                remotePort = int.Parse(reader.ReadLine());
                totalSlots = int.Parse(reader.ReadLine());
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
                    writer.WriteLine("PING");
                    string response = reader.ReadLine();
                    int slots = int.Parse(response);
                    if (slots != totalSlots)
                        Dispose();
                }
                catch
                {
                    Dispose();
                }   
            }, null, 0, 1000);
        }

        public void Dispose()
        {
            writer.Dispose();
            reader.Dispose();
            networkStream.Dispose();
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
