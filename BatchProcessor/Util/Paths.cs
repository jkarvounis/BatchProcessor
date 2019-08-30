using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcessor.Util
{
    public static class Paths
    {
        public static readonly string SETTINGS_FILE = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "BatchProcessor", "worker_settings.json");

        public static readonly string TEMP_DIR = Path.Combine(Path.GetTempPath(), "BatchProcessorWorker");

        static Paths()
        {
            FileUtil.TryDeleteDirectory(TEMP_DIR);

            if (!Directory.Exists(TEMP_DIR))
                Directory.CreateDirectory(TEMP_DIR);
        }
    }
}
