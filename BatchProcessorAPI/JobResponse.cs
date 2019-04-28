using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcessorAPI
{
    public class JobResponse
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public bool Completed { get; set; }
        public byte[] ReturnFile { get; set; }
        public string ConsoleOutput { get; set; }
        public string ConsoleError { get; set; }
    }
}
