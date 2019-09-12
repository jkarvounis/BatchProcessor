using BatchProcessorServer.Models;
using Nancy;
using System.Linq;
using BatchProcessorServer.Util;
using BatchProcessorServer.Data;
using System.IO;
using Nancy.Responses;

namespace BatchProcessorServer.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get("/", parameters =>
            {
                var workers = DB.GetWorkerInfo();
                int queueCount = DB.QueueCount();
                
                int payloads = System.IO.Directory.EnumerateFiles(Paths.TEMP_DIR).Count();

                StatusModel model = new StatusModel(workers, queueCount, payloads);

                return View["index", model];
            });

            Get("/setup.exe", parameters =>
            {
                if (File.Exists(Paths.INSTALLER))
                {
                    return new StreamResponse(() => File.OpenRead(Paths.INSTALLER), MimeTypes.GetMimeType(Paths.INSTALLER));
                }

                return HttpStatusCode.NotFound;
            });
        }
    }
}