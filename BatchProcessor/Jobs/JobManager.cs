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


        public JobManager(ProcessorSettings settings)
        {
            client = new RestClient($"http://{settings.ServerAddress}:{settings.ServerPort}")
                .UseSerializer(() => new JsonNetSerializer());

            workerID = Guid.NewGuid();

            modules = new List<JobModule>(settings.LocalSlots);
            for (int i = 0; i < settings.LocalSlots; i++)
                modules.Add(new JobModule(client, workerID, i));

            heatbeatTimer = new System.Timers.Timer(5000);
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
            heatbeatTimer.Stop();

            foreach (JobModule module in modules)
                module.Dispose();
        }

        private bool Register()
        {
            try
            {
                var request = new RestRequest($"register/{workerID}/{modules.Count}");
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
