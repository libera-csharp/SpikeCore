using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using SpikeCore.Domain;

namespace SpikeCore.Web
{
    public class Program
    {        
        public static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var tokenHolder = new WebHostCancellationTokenHolder(cancellationTokenSource);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("App Name", "SpikeCore")
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    cancellationTokenSource.Cancel();

                    Log.Information("Stopping.");
                };

                Log.Information("Running, press CTRL-C to stop the bot.");

                await CreateHostBuilder(args).ConfigureServices(servicesCollection => { servicesCollection.AddSingleton(tokenHolder); }).Build().RunAsync(cancellationTokenSource.Token);
                Log.Information("The bot has successfully stopped.");
            }
            finally
            {
                Log.CloseAndFlush();                
            }            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("App Name", "SpikeCore"))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}