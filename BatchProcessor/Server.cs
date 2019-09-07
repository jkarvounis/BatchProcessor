using BatchProcessor.Jobs;
using BatchProcessor.Util;
using Newtonsoft.Json;
using System;
using System.IO;

namespace BatchProcessor
{
    public class Server
    {
        public class Settings
        {
            public string ServerAddress { get; set; }
            public int ServerPort { get; set; }
            public int LocalSlots { get; set; }
            public int HeartbeatMs { get; set; }

            public Settings()
            {
                ServerAddress = "localhost";
                ServerPort = 1200;
                LocalSlots = System.Environment.ProcessorCount - 1;
                HeartbeatMs = 5000;
            }

            public string GetURI()
            {
                return $"http://{ServerAddress}:{ServerPort}";
            }

            public override string ToString()
            {
                return $"[Server: {GetURI()}, Slots:{LocalSlots}, HeartbeatMs: {HeartbeatMs}]";
            }

            public void Save(string path)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
            }

            public static Settings Load(string path)
            {
                if (File.Exists(path))
                    return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));

                Settings defaultSettings = new Settings();
                defaultSettings.Save(path);
                return defaultSettings;
            }
        }

        Settings settings = null;
        JobManager manager;

        public Server()
        {
            Console.WriteLine("Server Created");

            settings = Settings.Load(Paths.SETTINGS_FILE);
            Console.WriteLine($"Loaded Settings: {settings}");
        }

        public void Start()
        {
            Console.WriteLine("Server Started");

            settings = Settings.Load(Paths.SETTINGS_FILE);
            Console.WriteLine($"Loaded Settings: {settings}");

            Paths.CleanupTemp();
            manager = new JobManager(settings.GetURI(), settings.LocalSlots, settings.HeartbeatMs);
        }

        public void Stop()
        {
            Console.WriteLine("Server Stopped");            
            if (manager != null)
            {
                manager.Dispose();
                manager = null;
            }
        }
    }
}
