using BatchProcessorServer.Data;
using BatchProcessorServer.Util;
using Nancy;
using Nancy.Responses;
using System.IO;
using System.Text.RegularExpressions;

namespace BatchProcessorServer.Modules
{
    public class RegisterModule : NancyModule
    {       
        public RegisterModule() : base("/register")
        {
            Put("/{workerID}/{slotCount}/{name}", async parameters =>
            {
                string workerName = System.Web.HttpUtility.UrlDecode(parameters.name);
                await DB.RegisterWorkerAsync(parameters.workerID, parameters.slotCount, workerName, "");
                return HttpStatusCode.OK;
            });

            Put("/{workerID}/{slotCount}/{name}/{details}", async parameters =>
            {
                string workerName = System.Web.HttpUtility.UrlDecode(parameters.name);
                byte[] d = System.Convert.FromBase64String(parameters.details);
                string details = System.Web.HttpUtility.UrlDecode(d, System.Text.Encoding.UTF8);
                string detailsHtml = Regex.Replace(details, @"\r\n?|\n", "<br />");
                await DB.RegisterWorkerAsync(parameters.workerID, parameters.slotCount, workerName, detailsHtml);
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
