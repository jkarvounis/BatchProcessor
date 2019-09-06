using RestSharp;
using RestSharp.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace BatchProcessor.Util
{
    public static class PayloadUtil
    {
        private static SemaphoreSlim locker = new SemaphoreSlim(1);
        private static Dictionary<Guid, SemaphoreSlim> directoryLocker = new Dictionary<Guid, SemaphoreSlim>();
        private static Dictionary<Guid, int> directoryCount = new Dictionary<Guid, int>();

        public static string GetPayload(IRestClient client, Guid payloadID, int threadID)
        {
            locker.Wait();
            if (!directoryLocker.ContainsKey(payloadID))
                directoryLocker.Add(payloadID, new SemaphoreSlim(1));
            if (!directoryCount.ContainsKey(payloadID))
                directoryCount.Add(payloadID, 1);
            else
                directoryCount[payloadID]++;
            SemaphoreSlim payloadLocker = directoryLocker[payloadID];
            locker.Release();

            payloadLocker.Wait();

            string tempFile = getPath(payloadID);
            string tempDirectory = getDirectory(payloadID, threadID);

            try
            {
                if (File.Exists(tempFile))
                {
                    payloadLocker.Release();

                    if (!Directory.Exists(tempDirectory))
                    {
                        Directory.CreateDirectory(tempDirectory);
                        ZipFile.ExtractToDirectory(tempFile, tempDirectory);
                    }

                    return tempDirectory;
                }

                var request = new RestRequest($"payload/{payloadID}");
                var response = client.ExecuteAsGet(request, "GET");

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    payloadLocker.Release();
                    return null;
                }
            
                response.RawBytes.SaveAs(tempFile);
                payloadLocker.Release();

                Directory.CreateDirectory(tempDirectory);
                ZipFile.ExtractToDirectory(tempFile, tempDirectory);

                return tempDirectory;
            }
            catch
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                return null;
            }
        }

        public static void ReleasePayload(Guid payloadID, int threadID)
        {
            locker.Wait();
            if (directoryCount.ContainsKey(payloadID))
                directoryCount[payloadID]--;
            locker.Release();

            string tempDirectory = getDirectory(payloadID, threadID);
            FileUtil.TryDeleteDirectory(tempDirectory);
        }

        public static void CleanupPayloads()
        {
            locker.Wait();
            var pairs = directoryCount.ToList();
            foreach (var p in pairs)
            {
                if (p.Value <= 0)
                {
                    string path = getPath(p.Key);
                    if (File.Exists(path))
                        File.Delete(path);
                    directoryLocker.Remove(p.Key);
                    directoryCount.Remove(p.Key);
                }
            }
            locker.Release();
        }

        private static string getPath(Guid payloadID)
        {
            return Path.Combine(Paths.TEMP_DIR, payloadID.ToString() + ".zip");
        }

        private static string getDirectory(Guid payloadID, int threadID)
        {
            return Path.Combine(Paths.TEMP_DIR, payloadID.ToString() + "-" + threadID);
        }
    }
}
