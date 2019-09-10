using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcessorServer.Data
{
    public class WorkerInfo
    {
        public Guid ID { get; set; }
        public int Slots { get; set; }
        public Dictionary<Guid, JobItem> JobList { get; set; }

        public WorkerInfo(Guid id)
        {
            ID = id;
            Slots = 1;
            JobList = new Dictionary<Guid, JobItem>();
        }

        public void AddJobItem(JobItem job)
        {
            JobList.Add(job.job.ID, job);
        }
    }
}
