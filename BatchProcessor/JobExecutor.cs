using BatchProcessorAPI;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace BatchProcessor
{
    public static class JobExecutor
    {
        public static JobResponse Execute(Job job)
        {
            string tempDirectory = null;
            JobResponse response = new JobResponse();
            response.ID = job.ID;
            response.Name = job.Name;
            response.Completed = false;

            try
            {
                if (job.ZipPayload != null && job.ZipPayload.Length > 0)
                {
                    tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    Directory.CreateDirectory(tempDirectory);
                    string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
                    try
                    {
                        File.WriteAllBytes(tempFile, job.ZipPayload);
                        ZipFile.ExtractToDirectory(tempFile, tempDirectory);
                    }
                    finally
                    {
                        File.Delete(tempFile);
                    }
                }

                using (Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();

                    startInfo.FileName = job.Command;

                    if (!string.IsNullOrEmpty(tempDirectory))
                    {
                        startInfo.WorkingDirectory = tempDirectory;
                        startInfo.FileName = Path.Combine(tempDirectory, job.Command);
                    }

                    if (!string.IsNullOrEmpty(job.CommandArguments))
                        startInfo.Arguments = job.CommandArguments;

                    startInfo.RedirectStandardError = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;

                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();

                    response.Completed = true;
                    response.ConsoleOutput = process.StandardOutput.ReadToEnd();
                    response.ConsoleError = process.StandardError.ReadToEnd();
                    response.ReturnFile = null;
                }

                if (!string.IsNullOrEmpty(job.ReturnFilename))
                {
                    if (File.Exists(Path.Combine(tempDirectory, job.ReturnFilename)))
                        response.ReturnFile = File.ReadAllBytes(Path.Combine(tempDirectory, job.ReturnFilename));
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
            finally
            {
                if (tempDirectory != null)
                {
                    Task.Run(()=>TryDeleteDirectory(tempDirectory));
                }
            }
        }

        // https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true
        public static async Task<bool> TryDeleteDirectory(string directoryPath, int maxRetries = 10, int millisecondsDelay = 30)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(directoryPath);
            if (maxRetries < 1)
                throw new ArgumentOutOfRangeException(nameof(maxRetries));
            if (millisecondsDelay < 1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));

            for (int i = 0; i < maxRetries; ++i)
            {
                try
                {
                    if (Directory.Exists(directoryPath))
                    {
                        Directory.Delete(directoryPath, true);
                    }

                    return true;
                }
                catch (IOException)
                {
                    await Task.Delay(millisecondsDelay);
                }
                catch (UnauthorizedAccessException)
                {
                    await Task.Delay(millisecondsDelay);
                }
            }

            return false;
        }

    }
}
