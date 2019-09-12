using System;
using System.Diagnostics;
using System.IO;

namespace BatchProcessorServer.Util
{
    public static class Paths
    {
        public static readonly string VERSION = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;

        public static readonly string INSTALLER = Path.Combine("Content", $"Batch Processor Setup {VERSION}.exe");

        public static readonly string SETTINGS_FILE = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "BatchProcessor", "server_settings.json");

        public static readonly string TEMP_DIR = Path.Combine(Path.GetTempPath(), "BatchProcessorServer");

        public static void CleanupTemp()
        {            
            FileUtil.TryDeleteDirectory(TEMP_DIR);

            if (!Directory.Exists(TEMP_DIR))
                Directory.CreateDirectory(TEMP_DIR);
        }
    }
}
