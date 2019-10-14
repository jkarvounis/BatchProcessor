using System.Collections.Generic;

namespace BatchProcessorServer.Models
{
    public class ChartModel
    {
        public List<KeyValuePair<long, int>> QueueSize { get; set; }
        public List<KeyValuePair<long, int>> PayloadCount { get; set; }
        public List<KeyValuePair<long, int>> TotalCount { get; set; }
        public List<KeyValuePair<long, int>> TotalCurrent { get; set; }
        public List<KeyValuePair<long, int>> TotalWorkers { get; set; }
    }
}
