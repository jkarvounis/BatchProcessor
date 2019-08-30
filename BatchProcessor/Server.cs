using BatchProcessor.Jobs;
using System;
using System.IO;

namespace BatchProcessor
{
    public class Server
    {
        public static readonly string SETTINGS_FILE = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "BatchProcessor", "settings.json");

        ProcessorSettings settings = new ProcessorSettings();

        JobManager manager;

        public Server()
        {
            Console.WriteLine("Server Created");
            settings = ProcessorSettings.LoadOrDefault(SETTINGS_FILE);
            Console.WriteLine($"Loaded Settings: {settings}");
        }

        public void Start()
        {
            Console.WriteLine("Server Started");
            manager = new JobManager(settings);
        }

        public void Stop()
        {
            Console.WriteLine("Server Stopped");            
            if (manager != null)
            {
                manager.Dispose();
                manager = null;
            }
        }
    }
}
