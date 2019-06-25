using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BatchProcessorAPI;

namespace BatchProcessor
{
    public class JobDispatcher : IJobManager
    {
        class JobCounter
        {
            public int Counter;
            public IJobManager Manager;

            public JobCounter(IJobManager manager)
            {
                Counter = 0;
                Manager = manager;
            }

            public double Usage => Manager.GetTotalSlots() <= 0 ? double.MaxValue : ((double)Counter / Manager.GetTotalSlots());
        }

        int key = 1;
        Dictionary<int, JobCounter> managers;
        TcpListener tcpListener;
        Thread listenerThread;
        bool isRunning;

        public JobDispatcher(int workerPort, int localSlots)
        {
            managers = new Dictionary<int, JobCounter>();
            if (localSlots > 0)
                managers.Add(0, new JobCounter(new LocalJobManager(localSlots)));

            isRunning = true;
            tcpListener = new TcpListener(IPAddress.Any, workerPort);
            listenerThread = new Thread(new ThreadStart(() =>
            {
                Console.WriteLine("JobDispatcher listenerThread started");
                try
                {
                    tcpListener.Start();
                    while (isRunning)
                    {
                        TcpClient remote = tcpListener.AcceptTcpClient();
                        Thread remoteHandler = new Thread(new ParameterizedThreadStart(HandleRemoteClient));
                        remoteHandler.Start(remote);
                    }
                }
                catch (Exception)
                { }
                Console.WriteLine("JobDispatcher listenerThread exited");
            }));
            listenerThread.Start();
        }

        ~JobDispatcher()
        {
            Dispose();
        }

        private void HandleRemoteClient(object r)
        {
            TcpClient tcpClient = (TcpClient)r;
            int remoteKey = ++key;
            RemoteJobManager remoteJobManager = new RemoteJobManager(tcpClient, () => RemoveMananger(remoteKey));
            lock (managers)
            {
                Console.WriteLine($"Added Worker {remoteKey}");
                managers.Add(remoteKey, new JobCounter(remoteJobManager));
            }
        }

        private void RemoveMananger(int key)
        {
            lock (managers)
            {
                Console.WriteLine($"Removed Worker {key}");
                managers.Remove(key);
            }
        }

        public int GetTotalSlots()
        {
            lock (managers)
                return managers.Values.Sum(m => m.Manager.GetTotalSlots());
        }

        public async Task<JobResponse> ProcessJob(Job job)
        {
            var m = GetNextManager();
            ++m.Counter;
            var result = await m.Manager.ProcessJob(job);
            --m.Counter;
            return result;
        }

        private JobCounter GetNextManager()
        {
            lock (managers)
            {
                if (managers.Count == 0)
                    return null;

                return managers.Values.Where(m => m.Usage < double.MaxValue).OrderByDescending(m => m.Usage).First();
            }
        }

        public void Dispose()
        {
            isRunning = false;
            if (listenerThread != null)
            {
                tcpListener.Stop();
                listenerThread = null;
            }
            lock (managers)
            {
                foreach (var m in managers.Values.ToList())
                    m.Manager.Dispose();
                managers.Clear();
            }
        }
    }
}
