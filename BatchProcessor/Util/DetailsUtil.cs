using System.Management;

namespace BatchProcessor.Util
{
    class DetailsUtil
    {
        public static string GetDetails()
        {
            string output = "";
            ManagementObjectSearcher myProcessorObject = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in myProcessorObject.Get())
            {
                try { output += ("" + obj["Name"]).Trim() + System.Environment.NewLine; } catch { }
                try
                {
                    double cpu = ulong.Parse("" + obj["CurrentClockSpeed"]) / 1000.0;
                    output += "CPU: " + cpu.ToString("N2") + " GHz" + System.Environment.NewLine;
                } catch
                { }
            }

            ManagementObjectSearcher myRAMObject = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in myRAMObject.Get())
            {
                try
                {
                    double freeM = ulong.Parse("" + obj["FreePhysicalMemory"]) / (1024.0 * 1024.0);
                    double totalM = ulong.Parse("" + obj["TotalVisibleMemorySize"]) / (1024.0 * 1024.0);
                    output += "RAM: " + freeM.ToString("N2") + " / " + totalM.ToString("N2") + " GB" + System.Environment.NewLine;
                }
                catch { }  
            }
            return output.Trim();
        }
    }
}
