using BatchProcessorServer.Util;
using Nancy;
using Nancy.Responses;
using System;
using System.IO;
using System.Linq;

namespace BatchProcessorServer.Modules
{
    public class PayloadModule : NancyModule
    {        
        public PayloadModule() : base("/payload")
        {
            Post("/", async _ =>
            {
                var postedFile = Request.Files.FirstOrDefault();
                Guid newID = Guid.NewGuid();
                string tempFile = Path.Combine(Paths.TEMP_DIR, newID.ToString() + ".zip");
                
                FileStream tempFileStream = new FileStream(tempFile, FileMode.CreateNew);
                await postedFile.Value.CopyToAsync(tempFileStream);
                await tempFileStream.FlushAsync();
                tempFileStream.Close();

                return newID;                                    
            });

            Get("/{payloadID}", parameters =>
            {
                Guid id = parameters.payloadID;
                string fileName = Path.Combine(Paths.TEMP_DIR, id.ToString() + ".zip");
                
                if (!File.Exists(fileName))
                    return HttpStatusCode.NotFound;

                return new StreamResponse(() => File.OpenRead(fileName), MimeTypes.GetMimeType(fileName));
            });

            Delete("/{payloadID}", parameters =>
            {
                Guid id = parameters.payloadID;
                string fileName = Path.Combine(Paths.TEMP_DIR, id.ToString() + ".zip");

                if (!File.Exists(fileName))
                    return HttpStatusCode.OK;

                File.Delete(fileName);

                if (!File.Exists(fileName))
                    return HttpStatusCode.OK;

                return HttpStatusCode.GatewayTimeout;
            });
        }

    }
}
