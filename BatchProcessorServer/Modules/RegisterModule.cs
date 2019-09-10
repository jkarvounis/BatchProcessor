using BatchProcessorServer.Data;
using Nancy;

namespace BatchProcessorServer.Modules
{
    public class RegisterModule : NancyModule
    {        
        public RegisterModule() : base("/register")
        {
            Put("/{workerID}/{slotCount}/{name}", async parameters =>
            {
                string workerName = System.Web.HttpUtility.UrlDecode(parameters.name);
                await DB.AddWorkerCount(parameters.workerID, parameters.slotCount, workerName);
                return HttpStatusCode.OK;
            });

            Get("/upgrade/{version}", parameters =>
            {
                return HttpStatusCode.NoContent;
            });
        }
    }
}
