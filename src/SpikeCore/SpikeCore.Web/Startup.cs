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
using SpikeCore.Irc.IrcDotNet;
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

        public void ConfigureServices(IServiceCollection services)
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

            services.AddTransient<IIrcClient, IrcClient>();
            services.AddTransient<IBot, Bot>();
            services.AddSingleton<IBotManager, BotManager>();
            
            services.AddSingleton(Configuration);
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
