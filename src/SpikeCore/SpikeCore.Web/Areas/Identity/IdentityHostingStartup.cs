﻿using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(SpikeCore.Web.Areas.Identity.IdentityHostingStartup))]
namespace SpikeCore.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}