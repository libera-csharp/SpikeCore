using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Rebus.Activation;
using Rebus.Config;
using Rebus.Transport.InMem;

namespace SpikeCore.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using (var activator = new BuiltinHandlerActivator())
            {
                var rebusBus = Configure
                    .With(activator)
                    .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "SpikeBus"))
                    .Start();

                var webHost = WebHost
                    .CreateDefaultBuilder(args)
                    .ConfigureServices(servicesCollection => servicesCollection.AddSingleton(rebusBus))
                    .UseStartup<Startup>()
                    .Build();

                var cancellationTokenSource = new CancellationTokenSource();

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;

                    cancellationTokenSource.Cancel();

                    Console.WriteLine("Stopping.");
                };

                Console.WriteLine("Running.");
                Console.WriteLine("CTRL-C to stop.");

                await webHost.RunAsync(cancellationTokenSource.Token);

                Console.WriteLine("Stopped.");
            }
        }
    }
}