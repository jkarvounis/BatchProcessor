using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace BatchProcessor
{
    public class JobWorker : IDisposable
    {        
        Thread listenerThread;
        bool isRunning;

        public JobWorker(string serverAddress, int workerPort, int jobPort, int totalSlots)
        {
            isRunning = true;

            listenerThread = new Thread(new ThreadStart(() =>
            {
                Console.WriteLine("JobWorker listenerThread started");
                while (isRunning)
                {                    
                    try
                    {
                        using (TcpClient tcpClient = new TcpClient(serverAddress, workerPort))
                        using (NetworkStream networkStream = tcpClient.GetStream())
                        using (StreamReader reader = new StreamReader(networkStream))
                        using (StreamWriter writer = new StreamWriter(networkStream))
                        {
                            Console.WriteLine("JobWorker connection established");
                            writer.AutoFlush = true;

                            writer.WriteLine(jobPort.ToString());
                            writer.WriteLine(totalSlots.ToString());

                            while (isRunning && tcpClient.Connected)
                            {
                                reader.ReadLine();
                                writer.WriteLine(totalSlots.ToString());
                            }
                        }
                    }
                    catch
                    { }
                    Console.WriteLine("JobWorker disconnected");
                }
                Console.WriteLine("JobWorker listenerThread exited");
            }));
        }

        public void Dispose()
        {
            isRunning = false;
            if (listenerThread != null)
            {
                listenerThread.Abort();
                listenerThread = null;
            }
        }
    }
}
