using BatchProcessorServer.Util;
using System;
using Topshelf;
using Topshelf.Nancy;
using Nancy.Hosting.Self;

namespace BatchProcessorServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                x.Service<Server>(s =>
                {
                    s.ConstructUsing(name => new Server());
                    s.WhenStarted(server => server.Start());
                    s.WhenStopped(server => server.Stop());
                    s.WithNancyEndpoint(x, c =>
                    {
                        Server.Settings settings = Server.Settings.Load(Paths.SETTINGS_FILE);                        
                        c.AddHost(port: settings.Port);
                        c.UseBootstrapper(new Bootstrapper());
                    });
                });
                x.StartAutomatically();
                x.RunAsLocalSystem();

                x.SetDescription("Batch Processor Server Service for running .NET jobs from remote clients");
                x.SetDisplayName("Batch Processor Server");
                x.SetServiceName("Batch Processor Server Service");
            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
