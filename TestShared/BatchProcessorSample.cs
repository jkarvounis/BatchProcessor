using BatchProcessorAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestShared
{
    public class BatchProcessorSample
    {
        private readonly static int SERVER_PORT = 1200;

        private readonly static int MAX_PARALLEL = 100;

        private readonly static object locker = new object();

        public static void Execute(string exeName, string server, TimeSpan? timeout, string[] args)
        {
            if (args.Length > 0)
            {
                // Case to handle for executing a single job.  Typically this should be a lengthy task to run on the remote side.

                int delaySeconds = int.TryParse(args[0], out int argValue) ? argValue + 5 : 10;

                // Console Output is reported back
                Console.WriteLine($"Test App - Got args [{string.Join(", ", args)}] - delay {delaySeconds} seconds");

                System.Threading.Thread.Sleep(delaySeconds * 1000);

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
                    jobs.Add(new Job($"Job-{i}", exeName, i.ToString(), "output.txt"));

                // Create Job Scheduler, point to server IP and port
                JobScheduler scheduler = new JobScheduler(server, SERVER_PORT, MAX_PARALLEL);
                if (timeout.HasValue)
                    scheduler.Timeout = timeout.Value;

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
