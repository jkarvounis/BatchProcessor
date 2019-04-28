using BatchProcessorAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace TestApp
{
    class Program
    {
        static object locker = new object();

        static void Main(string[] args)
        {            
            if (args.Length > 0)
            {          
                // Case to handle for executing a single job.  Typically this should be a lengthy task to run on the remote side.

                // Console Output is reported back
                Console.WriteLine("Test App - Got arg: " + String.Join(", ", args));

                // A single select output file is reported back
                File.WriteAllText("output.txt", "Test App File Contents " + String.Join(", ", args));
            }
            else
            {
                // Main entry to handle requesting jobs
                Console.WriteLine("Test App");

                // Convert current working directory into payload for job execution
                byte[] payload = PayloadUtil.CreatePayloadWithWorkingDirectory();

                // Create 100 sample jobs
                // Command must be and executable in the contents of the payload
                // Output file is optional but must be a locally reported file after executing the command
                // Arguments are optional
                List<Job> jobs = new List<Job>();
                for (int i = 0; i < 100; i++)
                    jobs.Add(new Job($"Job-{i}", "TestApp.exe", i.ToString(), payload, "output.txt"));

                // Create Job Scheduler, point to server IP and port
                JobScheduler scheduler = new JobScheduler("127.0.0.1", 1200);

                // Schedule all 100 jobs in parallel, per completed job - print output
                int completed = 0;
                Console.WriteLine("Sending Jobs");                
                scheduler.ScheduleAll(jobs, response => 
                {
                    // A lock to make output look nice
                    lock (locker)
                    {                      
                        Console.WriteLine($"Job Response {response.Completed}: {response.Name}");
                        Console.WriteLine($"File: [{System.Text.Encoding.Default.GetString(response.ReturnFile)}] Output: [{response.ConsoleOutput}] Error: [{response.ConsoleError}]");
                        if (response.Completed)
                            completed++;
                    }
                });

                // Wait for user input before exiting
                Console.WriteLine($"Done Sending Jobs - Completed: {completed}");
                Console.ReadLine();
            }
        }
    }
}
