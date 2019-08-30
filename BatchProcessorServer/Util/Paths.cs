using System;
using System.IO;

namespace BatchProcessorServer.Util
{
    public static class Paths
    {
        public static readonly string SETTINGS_FILE = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "BatchProcessor", "server_settings.json");

        public static readonly string TEMP_DIR = Path.Combine(Path.GetTempPath(), "BatchProcessorServer");

        static Paths()
        {            
            FileUtil.TryDeleteDirectory(TEMP_DIR);

            if (!Directory.Exists(TEMP_DIR))
                Directory.CreateDirectory(TEMP_DIR);
        }
    }
}
