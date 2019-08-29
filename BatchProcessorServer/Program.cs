using System;
using BatchProcessorServer.Modules;
using Nancy.Hosting.Self;

namespace BatchProcessorServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri =
                new Uri("http://localhost:1200");

            using (var host = new NancyHost(uri))
            using (System.Timers.Timer timer = new System.Timers.Timer(10000))
            {
                timer.Elapsed += Timer_Elapsed;                
                host.Start();
                timer.Start();
                

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();

                timer.Stop();
            }
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            JobModule.RecoverBadJobs();
            Console.WriteLine("Recovering Jobs...");
        }
    }
}
