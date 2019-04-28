using System;
using System.IO;
using System.IO.Compression;

namespace BatchProcessorAPI
{
    public static class PayloadUtil
    {
        public static byte[] CreatePayloadWithWorkingDirectory()
        {
            string workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return CreatePayloadAtDirectory(workingDirectory);
        }

        public static byte[] CreatePayloadAtDirectory(string directory)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
            try
            {
                ZipFile.CreateFromDirectory(directory, tempFile);
                return File.ReadAllBytes(tempFile);
            }
            finally
            {
                File.Delete(tempFile);
            }            
        }
    }
}
