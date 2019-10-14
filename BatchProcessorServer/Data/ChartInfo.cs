using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcessorServer.Data
{
    public class ChartInfo
    {
        public int QueueSize { get; private set; }
        public int PayloadCount { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalCurrent { get; private set; }
        public int TotalWorkers { get; private set; }

        public ChartInfo(int queueSize, int payloadCount, int totalCount, int totalCurrent, int totalWorkers)
        {
            QueueSize = queueSize;
            PayloadCount = payloadCount;
            TotalCount = totalCount;
            TotalCurrent = totalCurrent;
            TotalWorkers = totalWorkers;
        }
    }
}
