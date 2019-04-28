using System;

namespace BatchProcessorAPI
{
    public class Job
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Command { get; set; }
        public string CommandArguments { get; set; }
        public byte[] ZipPayload { get; set; }
        public string ReturnFilename { get; set; }

        /// <summary>
        /// Create a job with a unique ID.  Send to JobScheduler to get a JobResponse
        /// </summary>
        /// <param name="name">Optional name for the job</param>
        /// <param name="command">Required command for an executable in the zipped payload</param>
        /// <param name="arguments">Optional arguments for the command</param>
        /// <param name="payload">Required zipped payload</param>
        /// <param name="returnFilename">Optional file to return in the response</param>
        public Job(string name, string command, string arguments, byte[] payload, string returnFilename)
        {
            ID = Guid.NewGuid();
            Name = name;
            Command = command;
            CommandArguments = arguments;
            ZipPayload = payload;
            ReturnFilename = returnFilename;
        }
    }
}
