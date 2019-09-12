using BatchProcessorServer.Data;
using BatchProcessorServer.Util;
using Nancy;
using Nancy.Responses;
using System.Diagnostics;
using System.IO;

namespace BatchProcessorServer.Modules
{
    public class RegisterModule : NancyModule
    {       
        public RegisterModule() : base("/register")
        {
            Put("/{workerID}/{slotCount}/{name}", async parameters =>
            {
                string workerName = System.Web.HttpUtility.UrlDecode(parameters.name);
                await DB.RegisterWorkerAsync(parameters.workerID, parameters.slotCount, workerName);
                return HttpStatusCode.OK;
            });
            
            Get("/upgrade/{version}", parameters =>
            {                
                if (parameters.version != Paths.VERSION)
                {
                    if (File.Exists(Paths.INSTALLER))
                    {
                        return new StreamResponse(() => File.OpenRead(Paths.INSTALLER), MimeTypes.GetMimeType(Paths.INSTALLER));
                    }
                }

                return HttpStatusCode.NoContent;
            });
        }
    }
}
