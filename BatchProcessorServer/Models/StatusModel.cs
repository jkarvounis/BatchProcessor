using BatchProcessorServer.Util;
using System.Collections.Generic;
using System.Linq;

namespace BatchProcessorServer.Models
{
    public class StatusModel
    {
        public List<WorkerModel> Workers { get; set; }        
        public ChartModel ChartData { get; set; }
        public int QueueSize { get; set; }
        public int PayloadCount { get; set; }
        public int TotalCount => Workers.Select(w => w.Count).Sum();
        public int TotalCurrent => Workers.Select(w => w.Current).Sum();
        public int TotalWorkers => Workers.Count;        
        public float TotalLoad => ((float)TotalCurrent / (float)TotalCount);
        public string TotalLoadFormatted => (TotalLoad * 100).ToString("##0.0") + "%";
        public string Version => Paths.VERSION;

        public StatusModel(List<WorkerModel> workers, ChartModel chartData, int queueSize, int payloadCount)
        {
            Workers = workers;
            ChartData = chartData;
            QueueSize = queueSize;
            PayloadCount = payloadCount;
        }
    }
}
