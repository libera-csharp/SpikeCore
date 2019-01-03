using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SpikeCore.Domain;

namespace SpikeCore.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var tokenHolder = new WebHostCancellationTokenHolder(cancellationTokenSource);
            
            var webHost = WebHost
                .CreateDefaultBuilder(args)
                .ConfigureServices(servicesCollection =>
                {
                    servicesCollection.AddSingleton(tokenHolder);
                })
                .UseStartup<Startup>()
                .Build();

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