using Nancy;
using System;
using System.Collections.Concurrent;

namespace BatchProcessorServer.Modules
{
    public class RegisterModule : NancyModule
    {
        public static ConcurrentDictionary<Guid, int> Workers = new ConcurrentDictionary<Guid, int>();        

        public RegisterModule() : base("/register")
        {
            Put("/{workerID}/{slotCount}", parameters =>
            {
                Workers.TryAdd(parameters.workerID, parameters.slotCount);
                return HttpStatusCode.OK;
            });

            Get("/upgrade/{version}", parameters =>
            {
                return HttpStatusCode.NoContent;
            });
        }
    }
}
