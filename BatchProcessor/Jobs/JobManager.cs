using BatchProcessor.Util;
using BatchProcessorAPI.RestUtil;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BatchProcessor.Jobs
{
    public class JobManager : IDisposable
    {
        IRestClient client;
        Guid workerID;

        List<JobModule> modules;

        System.Timers.Timer heatbeatTimer;

        public JobManager(string serverUri, int localSlots, int heartbeatMs)
        {
            client = new RestClient(serverUri)
                .UseSerializer(() => new JsonNetSerializer());

            workerID = Guid.NewGuid();
            Console.WriteLine($"WorkerID = {workerID}");

            modules = new List<JobModule>(localSlots);
            for (int i = 0; i < localSlots; i++)
                modules.Add(new JobModule(client, workerID, i));

            heatbeatTimer = new System.Timers.Timer(heartbeatMs);
            heatbeatTimer.Elapsed += HeatbeatTimer_Elapsed;
            heatbeatTimer.Start();
        }

        private void HeatbeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool online = Register();

            foreach (JobModule module in modules)
                module.SendHeartbeat();

            int jobCount = modules.Count(m => m.HasJob());

            if (jobCount == 0)
                PayloadUtil.CleanupPayloads();

            Console.WriteLine($"Status: {(online ? "ONLINE" : "OFFLINE")} - {jobCount}/{modules.Count} Jobs");
        }

        public void Dispose()
        {
            if (heatbeatTimer != null)
            {
                heatbeatTimer.Stop();
                heatbeatTimer = null;
            }

            foreach (JobModule module in modules)
                module.Dispose();

            modules.Clear();
        }

        private bool Register()
        {
            try
            {
                string name = System.Web.HttpUtility.UrlEncode(Environment.MachineName);
                var request = new RestRequest($"register/{workerID}/{modules.Count}/{name}");
                var response = client.Put(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    return true;
            }
            catch
            {
            }

            return false;
        }
    }
}
