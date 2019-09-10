using BatchProcessorServer.Data;
using Nancy;

namespace BatchProcessorServer.Modules
{
    public class RegisterModule : NancyModule
    {        
        public RegisterModule() : base("/register")
        {
            Put("/{workerID}/{slotCount}", async parameters =>
            {
                await DB.AddWorkerCount(parameters.workerID, parameters.slotCount);
                return HttpStatusCode.OK;
            });

            Get("/upgrade/{version}", parameters =>
            {
                return HttpStatusCode.NoContent;
            });
        }
    }
}
