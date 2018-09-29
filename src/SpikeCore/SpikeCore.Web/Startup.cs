using System;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SpikeCore.Data;
using SpikeCore.Data.Models;
using SpikeCore.Irc;
using SpikeCore.Irc.Configuration;
using SpikeCore.Irc.IrcDotNet;
using SpikeCore.MessageBus.Foundatio.AutofacIntergration;
using SpikeCore.Web.Configuration;
using SpikeCore.Web.Hubs;
using SpikeCore.Web.Services;

namespace SpikeCore.Web
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private WebConfig WebConfig { get; } = new WebConfig();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Configuration.GetSection("Web").Bind(WebConfig);
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (WebConfig.Enabled)
            {
                services.AddSignalR();

                services.Configure<CookiePolicyOptions>(options =>
                {
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });
            }

            services.AddDbContext<SpikeCoreDbContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("SpikeCoreDbContextConnection"));
            });

            services
                .AddDefaultIdentity<SpikeCoreUser>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<SpikeCoreDbContext>();

            if (WebConfig.Enabled)
            {
                services
                    .AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            }

            var containerBuilder = new ContainerBuilder();

            containerBuilder.Populate(services);

            var botConfig = new BotConfig();
            Configuration.GetSection("Bot").Bind(botConfig);
            containerBuilder.RegisterInstance(botConfig);

            containerBuilder
                .RegisterFoundatio();

            containerBuilder
                .RegisterType<Bot>()
                .As<IBot>()
                .SingleInstance();
            
            containerBuilder
                .RegisterType<IrcClient>()
                .As<IIrcClient>()
                .SingleInstance();           
            
            containerBuilder
                .RegisterType<BotManager>()
                .As<IBotManager>()
                .SingleInstance();

            var container = containerBuilder.Build();

            // Grab an instance of IBot so that it gets activated.
            // We don't need to keep hold of it, it's a singleton.
            // Using AutoActivate meant RegisterFoundatio wasn't able to hook Activated before activation.
            var bot = container.Resolve<IBot>();

            return new AutofacServiceProvider(container);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (WebConfig.Enabled)
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<TestHub>("/hubs/test");
                });

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseDatabaseErrorPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseCookiePolicy();

                app.UseAuthentication();

                app.UseMvc(routes =>
                {
                    routes.MapRoute
                    (
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}"
                    );
                });
            }
        }
    }
}
