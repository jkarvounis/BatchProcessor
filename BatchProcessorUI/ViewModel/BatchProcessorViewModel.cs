using BatchProcessorUI.Model;
using BatchProcessorUI.Util;
using System.Diagnostics;
using System.Windows.Input;

namespace BatchProcessorUI.ViewModel
{
    public class BatchProcessorViewModel
    {
        Process process = null;

        public ICommand Start { get; private set; }
        public ICommand Stop { get; private set; }

        public ICommand Save { get; private set; }
        public ICommand Load { get; private set; }

        public BatchProcessorState State { get; set; }

        public BatchProcessorViewModel()
        {
            State = new BatchProcessorState();

            Start = new ButtonCommand(() =>
            {
                State.ConsoleText = "Starting BatchProcessor.exe...";

                process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "BatchProcessor.exe";

                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                process.StartInfo = startInfo;
                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            });


            Stop = new ButtonCommand(() =>
            {
                if (process != null)
                {
                    process.StandardInput.WriteLine("Done");
                    process.Close();
                    process = null;
                    State.ConsoleText = "Stopping BatchProcessor.exe...";
                }
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

