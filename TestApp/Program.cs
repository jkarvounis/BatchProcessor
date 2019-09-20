using BatchProcessorAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace TestApp
{
    class Program
    {
        private readonly static string SERVER_IP = "localhost";
        private readonly static int SERVER_PORT = 1200;

        private readonly static int MAX_PARALLEL = 100;
        private readonly static int PROCESS_DELAY_MS = 1000;

        static object locker = new object();

        static void Main(string[] args)
        {            
            if (args.Length > 0)
            {          
                // Case to handle for executing a single job.  Typically this should be a lengthy task to run on the remote side.

                // Console Output is reported back
                Console.WriteLine("Test App - Got arg: " + String.Join(", ", args));

                System.Threading.Thread.Sleep(PROCESS_DELAY_MS);

                // A single select output file is reported back
                File.WriteAllText("output.txt", "Test App File Contents " + String.Join(", ", args));
            }
            else
            {
                // Main entry to handle requesting jobs
                Console.WriteLine("Test App");                

                // Create 20 sample jobs
                // Command must be and executable in the contents of the payload
                // Output file is optional but must be a locally reported file after executing the command
                // Arguments are optional
                List<Job> jobs = new List<Job>();
                for (int i = 0; i < MAX_PARALLEL; i++)
                    jobs.Add(new Job($"Job-{i}", "TestApp.exe", i.ToString(), "output.txt"));

                // Create Job Scheduler, point to server IP and port
                JobScheduler scheduler = new JobScheduler(SERVER_IP, SERVER_PORT, MAX_PARALLEL);

                // Convert current working directory into payload for job execution                
                string payloadFile = PayloadUtil.CreatePayloadFileWithWorkingDirectory();
                
                Console.WriteLine("Sending Payload");

                // Upload current payload to the cluster
                if (scheduler.UploadPayload(payloadFile))
                {
                    int completed = 0;
                    Console.WriteLine("Sending Jobs");

                    // Schedule all 100 jobs in parallel, per completed job - print output
                    var results = scheduler.ScheduleAll(jobs, response =>
                    {
                        // A lock to make output look nice
                        lock (locker)
                        {
                            Console.WriteLine($"Job Response {response.Completed}: {response.Name}");
                            string returnFile = "Empty";
                            if (response.ReturnFile != null)
                                returnFile = System.Text.Encoding.Default.GetString(response.ReturnFile);
                            Console.WriteLine($"File: [{returnFile}] Output: [{response.ConsoleOutput}] Error: [{response.ConsoleError}]");
                            if (response.Completed)
                                completed++;
                        }
                    });                    

                    // Wait for user input before exiting
                    Console.WriteLine($"Done Sending Jobs - Completed: {completed}");
                    Console.WriteLine($"Job Results - Completed: {results.Count(x => x.Completed)}");

                    bool removed = scheduler.RemovePayload();
                    Console.WriteLine($"Removed Payload: {removed}");
                }
                else
                {
                    Console.WriteLine($"Failed to upload payload");
                }
                

                Console.WriteLine("Press ENTER to exit.");
                Console.ReadLine();
            }
        }
    }
}
