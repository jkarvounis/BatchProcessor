using RestSharp;
using RestSharp.Extensions;
using System;
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

        public static string GetPayload(IRestClient client, Guid payloadID)
        {
            locker.Wait();
            if (!directoryLocker.ContainsKey(payloadID))
                directoryLocker.Add(payloadID, new SemaphoreSlim(1));
            SemaphoreSlim payloadLocker = directoryLocker[payloadID];
            locker.Release();

            payloadLocker.Wait();

            string tempDirectory = Path.Combine(Paths.TEMP_DIR, payloadID.ToString());
            if (Directory.Exists(tempDirectory))
            {
                payloadLocker.Release();
                return tempDirectory;
            }

            var request = new RestRequest($"payload/{payloadID}");
            var response = client.ExecuteAsGet(request, "GET");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                payloadLocker.Release();
                return null;
            }

            string tempFile = Path.Combine(Paths.TEMP_DIR, payloadID.ToString() + ".zip");
            try
            {
                response.RawBytes.SaveAs(tempFile);            
                Directory.CreateDirectory(tempDirectory);
                ZipFile.ExtractToDirectory(tempFile, tempDirectory);
            }
            finally
            {
                File.Delete(tempFile);
            }

            payloadLocker.Release();
            return tempDirectory;
        }

        public static void CleanupPayloads()
        {

        }
    }
}
