using System;
using System.Collections.Generic;
using System.Linq;
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
using SpikeCore.Irc.Irc4NetButSmarter;
using SpikeCore.MessageBus.Foundatio.AutofacIntegration;
using SpikeCore.Modules;
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

            var ircConnectionConfig = new IrcConnectionConfig();
            Configuration.GetSection("IrcConnection").Bind(ircConnectionConfig);
            containerBuilder.RegisterInstance(ircConnectionConfig);
            
            var moduleConfiguration = new ModuleConfiguration();
            Configuration.GetSection("Modules").Bind(moduleConfiguration);
            containerBuilder.RegisterInstance(moduleConfiguration);

            containerBuilder
                .RegisterFoundatio();

            containerBuilder
                .RegisterType<IrcConnection>()
                .As<IIrcConnection>()
                .SingleInstance();
            
            containerBuilder
                .RegisterType<IrcClient>()
                .As<IIrcClient>()
                .SingleInstance();
            
            containerBuilder
                .RegisterType<SignalRMessageBusConnector>()
                .As<ISignalRMessageBusConnector>()
                .SingleInstance();

            containerBuilder
                .RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().Name == "SpikeCore"))
                .Where(t => t.Name.EndsWith("Module"))
                .As<IModule>()
                .PropertiesAutowired()
                .SingleInstance();
            
            var container = containerBuilder.Build();

            // Grab an instance of IBot so that it gets activated.
            // We don't need to keep hold of it, it's a singleton.
            // Using AutoActivate meant RegisterFoundatio wasn't able to hook Activated before activation.
            container.Resolve<IIrcConnection>();
            
            // We also need to resolve all of our modules.
            container.Resolve<IEnumerable<IModule>>();

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
