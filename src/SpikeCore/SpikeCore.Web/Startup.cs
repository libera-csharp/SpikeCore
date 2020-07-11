using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
using SpikeCore.Web.TokenProviders;

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
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<SpikeCoreDbContext>()
                .AddDefaultTokenProviders()
                .AddPasswordlessLoginTokenProvider()
                .AddUserStore<SpikeCoreUserStore>();

            if (WebConfig.Enabled)
            {
                services
                    .AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            }

            services.AddHttpClient();

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
            
            // Register this as a singleton so both our modules and ASP.NET infrastructure get the same instance.
            // Removing this causes our modules and controllers to get different instances of a UserManager, which, 
            // due to EF means that making updates via one is not available to the other.
            containerBuilder.RegisterType<UserManager<SpikeCoreUser>>()
                .As<UserManager<SpikeCoreUser>>()
                .PropertiesAutowired()
                .SingleInstance();

            containerBuilder
                .RegisterType<IrcConnection>()
                .As<IIrcConnection>()
                .SingleInstance();

            containerBuilder
                .RegisterType<IrcClient>()
                .As<IIrcClient>()
                .PropertiesAutowired()
                .SingleInstance();

            containerBuilder
                .RegisterType<SignalRMessageBusConnector>()
                .As<ISignalRMessageBusConnector>()
                .SingleInstance();

            containerBuilder
                .RegisterType<LoggingListener>()
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
            container.Resolve<LoggingListener>();

            return new AutofacServiceProvider(container);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (WebConfig.Enabled)
            {
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

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<TestHub>("/hubs/test");
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                    endpoints.MapRazorPages();
                });
            }
        }
    }
}
