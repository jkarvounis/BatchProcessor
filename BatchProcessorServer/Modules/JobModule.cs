using BatchProcessorAPI;
using BatchProcessorServer.Data;
using Nancy;
using Nancy.ModelBinding;

namespace BatchProcessorServer.Modules
{
    public class JobModule : NancyModule
    {                        
        public JobModule() : base("/job")
        {
            Post("/", async _ =>
            {
                Job job = this.Bind<Job>();
                JobItem jobItem = new JobItem(job);
                await DB.QueueJobItemAsync(jobItem);

                await jobItem.semaphore.WaitAsync();
                return jobItem.response;
            });

            Get("/queue/{workerID}", async parameters =>
            {
                JobItem jobItem = await DB.DeqeueueJobItemAsync(parameters.workerID);
                if (jobItem == null)
                    return HttpStatusCode.NoContent;
                return jobItem;
            });

            Put("/{jobID}/{workerID}/heatbeat", async parameters =>
            {
                bool success = await DB.StoreHeartbeat(parameters.workerID, parameters.jobID);
                if (success)
                    return HttpStatusCode.OK;
                else
                    return HttpStatusCode.NotFound;
            });

            Delete("/{jobID}/{workerID}", async parameters =>
            {
                JobItem jobItem = await DB.RemoveJobForResponse(parameters.workerID, parameters.jobID);
                
                if (jobItem == null)
                    return HttpStatusCode.NotFound;

                jobItem.response = this.Bind<JobResponse>();
                jobItem.semaphore.Release();

                return HttpStatusCode.OK;
            });
        }
    }
}
