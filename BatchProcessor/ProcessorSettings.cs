using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;

namespace BatchProcessor
{
    public class ProcessorSettings
    {
        public int LocalSlots { get; set; }
        public int JobServerPort { get; set; }
        public int WorkerPort { get; set; }
        public string ServerAddress { get; set; }
        public bool IsServer { get; set; }

        public ProcessorSettings()
        {
            LocalSlots = Environment.ProcessorCount - 1;
            JobServerPort = 1200;
            WorkerPort = 1201;
            ServerAddress = "";
            IsServer = true;
        }

        public override string ToString()
        {
            return $"Settings:\nLocalSlots = {LocalSlots}\nJobServerPort = {JobServerPort}\nWorkerPort = {WorkerPort}\nServerAddress = {ServerAddress}\nIsServer = {IsServer}";
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this));
        }

        public static ProcessorSettings Load(string path)
        {
            return JsonConvert.DeserializeObject<ProcessorSettings>(File.ReadAllText(path));
        }

        public static ProcessorSettings LoadOrDefault(string path)
        {
            if (File.Exists(path))
                return JsonConvert.DeserializeObject<ProcessorSettings>(File.ReadAllText(path));
            return new ProcessorSettings();
        }
    }
}
