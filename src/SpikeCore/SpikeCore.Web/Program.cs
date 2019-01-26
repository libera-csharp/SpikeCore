using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SpikeCore.Domain;

namespace SpikeCore.Web
{
    public class Program
    {
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path: "appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        
        public static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var tokenHolder = new WebHostCancellationTokenHolder(cancellationTokenSource);
            
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.WithProperty("App Name", "SpikeCore")
                .CreateLogger();

            try
            {
                var webHost = WebHost
                    .CreateDefaultBuilder(args)
                    .ConfigureServices(servicesCollection => { servicesCollection.AddSingleton(tokenHolder); })
                    .UseStartup<Startup>()
                    .UseSerilog()
                    .Build();

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    cancellationTokenSource.Cancel();

                    Log.Information("Stopping.");
                };

                Log.Information("Running, press CTRL-C to stop the bot.");

                await webHost.RunAsync(cancellationTokenSource.Token);
                Log.Information("The bot has successfully stopped.");
            }
            finally
            {
                Log.CloseAndFlush();                
            }            
        }
    }
}