using BatchProcessor.Util;
using BatchProcessorAPI.RestUtil;
using RestSharp;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BatchProcessor.Jobs
{
    public class JobManager : IDisposable
    {
        IRestClient client;
        Guid workerID;

        List<JobModule> modules;
        bool tryUpdate;
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

            tryUpdate = true;

            heatbeatTimer = new System.Timers.Timer(heartbeatMs);
            heatbeatTimer.Elapsed += HeatbeatTimer_Elapsed;
            heatbeatTimer.Start();
        }

        private void HeatbeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {            
            bool online = Register();

            if (online && tryUpdate)
                TryUpdate();

            tryUpdate = !online;

            foreach (JobModule module in modules)
                module.SendHeartbeat();

            int jobCount = modules.Count(m => m.HasJob());

            if (jobCount == 0)
            {
                PayloadUtil.CleanupPayloads();
                tryUpdate = true;
            }

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

        static string version = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;
        private void TryUpdate()
        {            
            try
            {                
                var request = new RestRequest($"register/upgrade/1.{version}");
                var response = client.Get(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string tempFile = Path.Combine(Paths.TEMP_DIR, "installer.exe");
                    response.RawBytes.SaveAs(tempFile);
                    Process.Start(tempFile, @"/S");
                }
            }
            catch
            {
            }
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
