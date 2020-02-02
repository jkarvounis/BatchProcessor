using System;
using System.Collections.Generic;

namespace BatchProcessorServer.Data
{
    public class WorkerInfo
    {
        public Guid ID { get; private set; }
        public string Name { get; private set; }
        public string Details { get; private set; }
        public int Slots { get; private set; }
        public Dictionary<Guid, JobItem> JobList { get; private set; }
        public DateTime RegistrationTime { get; private set; }

        public WorkerInfo(Guid id)
        {
            ID = id;
            Name = "Not Set";
            Details = "";
            Slots = 1;
            JobList = new Dictionary<Guid, JobItem>();
            RegistrationTime = DateTime.UtcNow;
        }

        public void AddJobItem(JobItem job)
        {
            JobList.Add(job.job.ID, job);
        }

        public void SetRegistrationInfo(int slots, string name, string details)
        {
            Slots = slots;
            Name = name;
            Details = details;
            RegistrationTime = DateTime.UtcNow;
        }
    }
}
