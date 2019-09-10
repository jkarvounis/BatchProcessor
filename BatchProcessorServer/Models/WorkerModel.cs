using System;

namespace BatchProcessorServer.Models
{
    public class WorkerModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int Current { get; set; }
        public float Load => ((float)Current / (float)Count);
    }
}
