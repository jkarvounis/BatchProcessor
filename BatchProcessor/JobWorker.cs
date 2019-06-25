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
                            writer.AutoFlush = true;

                            writer.WriteLine(jobPort.ToString());
                            writer.WriteLine(totalSlots.ToString());


                            string response = reader.ReadLine();
                            Console.WriteLine("JobWorker Pending - " + response);
                            if (response != "PING")
                            {
                                System.Threading.Thread.Sleep(1000);    
                                continue;
                            }

                            writer.WriteLine(totalSlots.ToString());

                            Console.WriteLine("JobWorker connection established");

                            while (isRunning && tcpClient.Connected && response == "PING")
                            {
                                response = reader.ReadLine();
                                writer.WriteLine(totalSlots.ToString());
                            }
                        }
                    }
                    catch
                    { 
                        System.Threading.Thread.Sleep(5000);    
                    }                    
                }
                Console.WriteLine("JobWorker listenerThread exited");
            }));
            listenerThread.Start();
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
