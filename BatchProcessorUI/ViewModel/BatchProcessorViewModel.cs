using BatchProcessorUI.Model;
using BatchProcessorUI.Util;
using System.Diagnostics;

namespace BatchProcessorUI.ViewModel
{
    public class BatchProcessorViewModel
    {
        Process process = null;

        public ButtonCommand Start { get; private set; }
        public ButtonCommand Stop { get; private set; }

        public ButtonCommand Install { get; private set; }
        public ButtonCommand Uninstall { get; private set; }

        public ButtonCommand StartService { get; private set; }
        public ButtonCommand StopService { get; private set; }

        public ButtonCommand Save { get; private set; }
        public ButtonCommand Load { get; private set; }

        public BatchProcessorState State { get; set; }

        public BatchProcessorViewModel()
        {
            State = new BatchProcessorState();

            Start = new ButtonCommand(() =>
            {
                State.ConsoleText = "Starting Local BatchProcessor...";

                RunProcess("", false);
            });

            Stop = new ButtonCommand(() =>
            {
                StopProcess();
            });

            Install = new ButtonCommand(() =>
            {
                State.ConsoleText = "Installing BatchProcessor Service...";

                RunProcess("install", true);
            });

            Uninstall = new ButtonCommand(() =>
            {
                State.ConsoleText = "Uninstalling BatchProcessor Service...";

                RunProcess("uninstall", true);
            });

            StartService = new ButtonCommand(() =>
            {
                State.ConsoleText = "Start BatchProcessor Service...";

                RunProcess("start", true);
            });

            StopService = new ButtonCommand(() =>
            {
                State.ConsoleText = "Stop BatchProcessor Service...";
                RunProcess("stop", true);
            });

            Save = new ButtonCommand(() =>
            {
                State.Save();
            });

            Load = new ButtonCommand(() =>
            {
                State.Load();
            });
        }

        private void RunProcess(string arguments, bool waitForExit)
        {
            StopProcess();

            process = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "BatchProcessor.exe",
                Arguments = arguments,

                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.StartInfo = startInfo;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (waitForExit)
            {
                process.WaitForExit();
                process = null;
            }
        }

        private void StopProcess()
        {
            if (process != null)
            {                
                process.StandardInput.Close();
                process.Kill();
                process.Dispose();
                process = null;
                State.ConsoleText += System.Environment.NewLine + "Stopping BatchProcessor...";
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            State.ConsoleText += System.Environment.NewLine + e.Data;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            State.ConsoleText += System.Environment.NewLine + e.Data;
        }
    }   
}

