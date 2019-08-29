using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcessorServer.Models
{
    public class StatusModel
    {
        public class WorkerModel
        {
            public Guid ID { get; set; }
            public int Count { get; set; }
            public int Current { get; set; }
            public float Load => ((float)Current / (float)Count);
        }

        public List<WorkerModel> Workers { get; set; }
        public int QueueSize { get; set; }
        public int PayloadCount { get; set; }
        public int TotalCount => Workers.Select(w => w.Count).Sum();
        public int TotalCurrent => Workers.Select(w => w.Current).Sum();
        public int TotalWorkers => Workers.Count;        
        public float TotalLoad => ((float)TotalCurrent / (float)TotalCount);

        public StatusModel(List<WorkerModel> workers, int queueSize, int payloadCount)
        {
            Workers = workers;
            QueueSize = queueSize;
            PayloadCount = payloadCount;
        }
    }
}
