using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SpikeCore.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var webHost = WebHost
                .CreateDefaultBuilder(args)
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