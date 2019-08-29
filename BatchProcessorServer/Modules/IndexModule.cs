using BatchProcessorServer.Models;
using Nancy;
using System.Linq;
using System.Collections.Generic;
using BatchProcessorServer.Util;

namespace BatchProcessorServer.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get("/", parameters =>
            {
                var workerLoad = JobModule.WorkerCounts();
                int queueCount = JobModule.QueueCount();

                List<StatusModel.WorkerModel> workers = new List<StatusModel.WorkerModel>();
                foreach (var workerPair in workerLoad)
                {
                    int count = -1;
                    if (!RegisterModule.Workers.TryGetValue(workerPair.Key, out count))
                        count = -1;

                    workers.Add(new StatusModel.WorkerModel()
                    {
                        ID = workerPair.Key,
                        Count = count,
                        Current = workerPair.Value
                    });
                }

                int payloads = System.IO.Directory.EnumerateFiles(Paths.TEMP_DIR).Count();

                StatusModel model = new StatusModel(workers, queueCount, payloads);

                return View["index", model];
            });
        }
    }
}