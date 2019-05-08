using System;
using System.IO;

namespace BatchProcessor
{
    public class Server
    {
        public static readonly string SETTINGS_FILE = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "BatchProcessor", "settings.json");

        ProcessorSettings settings = new ProcessorSettings();

        JobListener jobListener = null;
        IJobManager jobManager = null;
        JobWorker jobWorker = null;

        public Server()
        {
            Console.WriteLine("Server Created");
            settings = ProcessorSettings.LoadOrDefault(SETTINGS_FILE);
            Console.WriteLine($"Loaded Settings: {settings}");
        }

        public void Start()
        {
            Console.WriteLine("Server Started");
            if (settings.IsServer)
            {
                jobManager = new JobDispatcher(settings.WorkerPort, settings.LocalSlots);
            }
            else
            {
                jobWorker = new JobWorker(settings.ServerAddress, settings.WorkerPort, settings.JobServerPort, settings.LocalSlots);
                jobManager = new LocalJobManager(settings.LocalSlots);
            }

            jobListener = new JobListener(settings.JobServerPort, jobManager);
        }

        public void Stop()
        {
            Console.WriteLine("Server Stopped");
            if (jobListener != null)
            {
                jobListener.Dispose();
                jobManager.Dispose();
                jobListener = null;
                jobManager = null;
            }
            if (jobWorker != null)
            {
                jobWorker.Dispose();
                jobWorker = null;
            }
        }
    }
}
