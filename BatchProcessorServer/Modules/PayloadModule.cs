using BatchProcessorServer.Data;
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
                return await DB.CreatePayload(postedFile.Value);                                    
            });

            Get("/{payloadID}", async parameters =>
            {
                Guid id = parameters.payloadID;
                string fileName = await DB.GetPayloadFile(id);

                if (fileName == null)
                    return HttpStatusCode.NotFound;

                return new StreamResponse(() => File.OpenRead(fileName), MimeTypes.GetMimeType(fileName));
            });

            Delete("/{payloadID}", async parameters =>
            {
                Guid id = parameters.payloadID;
                await DB.DeletePayload(id);
                return HttpStatusCode.OK;
            });
        }

    }
}
