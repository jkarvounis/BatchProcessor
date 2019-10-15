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
                var chartData = DB.GetChartData();
                int queueCount = DB.QueueCount();
                int payloads = DB.GetPayloadCount();

                StatusModel model = new StatusModel(workers, chartData, queueCount, payloads);

                var outputView = View["index", model];
                return outputView;
            });

            Get("/update/status", parameters =>
            {
                var workers = DB.GetWorkerInfo();
                var chartData = DB.GetChartData();
                int queueCount = DB.QueueCount();
                int payloads = DB.GetPayloadCount();

                StatusModel model = new StatusModel(workers, chartData, queueCount, payloads);

                return model;
            });

            Get("/download/setup.exe", parameters =>
            {
                if (File.Exists(Paths.INSTALLER))
                {
                    return new StreamResponse(() => File.OpenRead(Paths.INSTALLER), MimeTypes.GetMimeType(Paths.INSTALLER));
                }

                return HttpStatusCode.NotFound;
            });
                                    
            Get("/reset", parameters =>
            {
                DB.Reset();
                return new RedirectResponse("/");
            });
        }
    }
}