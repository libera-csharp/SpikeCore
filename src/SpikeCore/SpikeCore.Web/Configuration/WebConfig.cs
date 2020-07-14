namespace SpikeCore.Web.Configuration
{
    public class WebConfig
    {
        public bool Enabled { get; set; }

        /// <summary>
        /// Provides a programmatic way of setting the <code>PathBase</code> for requests. This is handy if you're using a setup with
        /// a reverse proxy that does path trimming. If this property is blank, no <code>PathBase</code> will be set.
        ///
        /// see https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-3.1#other-proxy-server-and-load-balancer-scenarios
        /// for details.
        /// </summary>
        public string PathBase { get; set; }
    }
}