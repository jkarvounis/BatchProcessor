using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcessor
{
    public class Program
    {
        public static readonly string SETTINGS_FILE = "settings.json";

        static void Main(string[] args)
        {
            ProcessorSettings settings = ProcessorSettings.LoadOrDefault(SETTINGS_FILE);

            JobListener jobListener = null;
            IJobManager jobManager = null;
            JobWorker jobWorker = null;

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

            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();

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
