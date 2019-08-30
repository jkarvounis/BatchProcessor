using BatchProcessorAPI;
using System;
using System.Diagnostics;
using System.IO;

namespace BatchProcessor.Jobs
{
    public static class JobExecutor
    {
        public static JobResponse Execute(Job job, string payloadDirectory)
        {            
            JobResponse response = new JobResponse();
            response.ID = job.ID;
            response.Name = job.Name;
            response.Completed = false;

            try
            {                
                using (Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();

                    startInfo.FileName = job.Command;

                    if (!string.IsNullOrEmpty(payloadDirectory))
                    {
                        startInfo.WorkingDirectory = payloadDirectory;
                        startInfo.FileName = Path.Combine(payloadDirectory, job.Command);
                    }

                    if (!string.IsNullOrEmpty(job.CommandArguments))
                        startInfo.Arguments = job.CommandArguments;

                    startInfo.RedirectStandardError = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;

                    process.StartInfo = startInfo;
                    process.Start();

                    // 1-hour auto-cleanup window
                    if (process.WaitForExit(3600000))
                    {
                        response.Completed = true;
                        response.ConsoleOutput = process.StandardOutput.ReadToEnd();
                        response.ConsoleError = process.StandardError.ReadToEnd();
                        response.ReturnFile = null;
                    }
                    else
                    {
                        response.Completed = false;
                        process.Kill();
                        response.ConsoleOutput = process.StandardOutput.ReadToEnd();
                        response.ConsoleError = process.StandardError.ReadToEnd();
                        response.ReturnFile = null;
                        return response;
                    }
                }

                if (!string.IsNullOrEmpty(job.ReturnFilename))
                {
                    if (File.Exists(Path.Combine(payloadDirectory, job.ReturnFilename)))
                        response.ReturnFile = File.ReadAllBytes(Path.Combine(payloadDirectory, job.ReturnFilename));
                    else
                        response.Completed = false;
                }

                return response;
            }
            catch (Exception e)
            {
                response.ConsoleError = "Caught Exception Executing Job: " + e.ToString();
                response.Completed = false;
                return response;
            }            
        }        
    }
}
