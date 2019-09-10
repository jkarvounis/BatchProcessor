using BatchProcessorServer.Data;
using BatchProcessorServer.Util;
using Nancy.Hosting.Self;
using Newtonsoft.Json;
using System;
using System.IO;

namespace BatchProcessorServer
{
    public class Server
    {
        public class Settings
        {
            public int Port { get; set; }
            public int HeartbeatMs { get; set; }

            public Settings()
            {
                Port = 1200;
                HeartbeatMs = 5000;
            }

            public override string ToString()
            {
                return $"[Port: {Port}, HeartbeatMs: {HeartbeatMs}]";
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
        NancyHost host = null;
        System.Timers.Timer timer = null;

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

            var uri = new Uri($"http://localhost:{settings.Port}");
            host = new NancyHost(uri);
            host.Start();

            timer = new System.Timers.Timer(2 * settings.HeartbeatMs);                
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public void Stop()
        {
            Console.WriteLine("Server Stopped");

            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            if (host != null)
            {
                host.Stop();
                host.Dispose();
                host = null;
            }
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DB.RecoverBadJobs();
        }
    }
}
