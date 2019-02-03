using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;
using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    /// <summary>
    /// Provides GeoIP-based functionality to the bot, with an optional IP score. The score is computed by a third party
    /// and the higher it is, the more likely it is to be a proxy, VPN, or known bad actor (i.e. related to attacks).
    /// </summary>
    /// 
    /// <remarks>
    /// In order to use this module, you will need to register for a free API key at <code>https://ipstack.com/</code>,
    /// and set it in your appsettings.json file under <code>Modules.GeoIpApiKey</code>.
    ///
    /// If you would also like the optional IP scoring, you will need to configure a contact email, and set it as
    /// <code>Modules.GeoIpIntelEmailAddress</code> in your appsettings.json. Please note that while IpIntel does not
    /// require you to register, they do require that you give them an email address so that they can contact you in the
    /// event of feature deprecation or naughty behavior (rather than outright blocking you immediately). It's a cool
    /// free service, please be respectful.
    /// </remarks>
    public class GeoIpModule : ModuleBase
    {
        private static readonly Regex Regex = new Regex(@"~geoip\s(.*)?");
        
        public override string Name => "GeoIp";
        public override string Description => "Looks up GeoIp information about a given IP";
        public override string Instructions => "geoip <ip>";

        private readonly IHttpClientFactory _httpClientFactory;

        public GeoIpModule(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task HandleMessageAsyncInternal(IrcPrivMessage request, CancellationToken cancellationToken)
        {
            var match = Regex.Match(request.Text);
            if (!string.IsNullOrEmpty(Configuration.GeoIpApiKey) && match.Success)
            {
                var ip = match.Groups[1].Value.Trim();
                Task<HttpResponseMessage> ipScoreResponse = null;

                // Only check proxy status if someone has configured a contact email. This is required by the API.
                if (!string.IsNullOrEmpty(Configuration.GeoIpIntelEmailAddress))
                {
                    // This will be slower than our GeoIP lookup, so let's kick it off first.
                    ipScoreResponse = LookUpIpScore(ip, cancellationToken);
                }

                var body = await LookUpGeoIp(ip);
                var parsedGeoIpResponse = JObject.Parse(body);

                // The GeoIP service will give us a 200 even if it failed. The only way we know is to look for a success
                // property that only seems to exist when the call fails.
                if (null == parsedGeoIpResponse["success"] || parsedGeoIpResponse["success"].Value<bool>())
                {
                    var city = parsedGeoIpResponse["city"].Value<string>() ?? "<unknown city>";                    
                    var country = parsedGeoIpResponse["country_name"].Value<string>() ?? "<unknown city>";                
                    var result = $"{ip} resolves to: {city}, {country}.";

                    // Assuming our setup has the proxy check enabled, let's give it some time to try and finish. If it
                    // doesn't come back within a reasonable period of time we'll respond without it.
                    if (null != ipScoreResponse && 
                        (await Task.WhenAny(ipScoreResponse, Task.Delay(2000, cancellationToken)) == ipScoreResponse))
                    {
                        var statusCode = ipScoreResponse.Result.StatusCode;
                        
                        // IPIntelligence asks that consumers respect a 429, and try not to call more than 2 queries/sec.
                        if (statusCode == HttpStatusCode.TooManyRequests)
                        {
                            Log.Warning("We've been throttled by IpIntelligence, adding a backoff...");
                            result += " Getipintel.net has throttled this request. Please wait a few seconds before trying another."; 
                        }

                        if (ipScoreResponse.Result.IsSuccessStatusCode)
                        {
                            var ipIntelResult = await ipScoreResponse.Result.Content.ReadAsStringAsync();
                            result += $" Getipintel.net scores this at {ipIntelResult} ({ParseIpScoreResponse(ipIntelResult)}).";   
                        }
                        else
                        {
                            // The API doesn't document any other response codes, but the 500 series always in play when it comes to HTTP...
                            result += " Getipintel.net lookup has failed, please check the logs.";
                            Log.Warning("Received a status of {StatusCode} from getipintel.net", statusCode);
                        }                                                                                              
                    }

                    await SendResponse(request, result); 
                }
                else
                {
                    Log.Warning("Failed to call our GeoIP service. Body: {Body}", body);                    
                    await SendResponse(request, "Failed calling the GeoIP service, please check the logs.");
                }                
            }
        }
        
        private static string ParseIpScoreResponse(string scoreResponse)
        {
            double.TryParse(scoreResponse, out var score);

            // Handle known error codes from the service.
            switch (score)
            {
                case double _ when Math.Abs(score - (-1)) < double.Epsilon || Math.Abs(score - (-2)) < double.Epsilon:
                    return "is an invalid IP";
                case double _ when Math.Abs(score - (-3)) < double.Epsilon:
                    return "is a non-public IP";
                case double _ when Math.Abs(score - (-4)) < double.Epsilon:
                    return "the service is offline for maintenance";
                case double _ when Math.Abs(score - (-5)) < double.Epsilon || Math.Abs(score - (-6)) < double.Epsilon:
                    return "the bot is misconfigured, or has been banned";
            }
            
            // Handle actual scores
            switch (score)
            {
                // Anything under .9 is considered "low risk"
                case double _ when score > .001 && score < .7:
                    return "most likely reputable";
                case double _ when score >= .7 && score < .90:
                    return "might be reputable";
                // At around .95 we start to have enough confidence that this might be a proxy.
                case double _ when score > .90 && score < .95:
                    return "probably not reputable";
                case double _ when score >= .95 && score < .99:
                    return "almost certainly not reputable";
                // Over .95 it seems likely that there's a proxy. And at anything over .99, we are very, very certain.
                default:
                    return score > .99 ? "is a known proxy/VPN/bad actor" : "is known to be reputable";
            }
        }

        private Task<string> LookUpGeoIp(string ip)
        {
            // API docs are available at https://ipstack.com/documentation.
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://api.ipstack.com");

            return client.GetStringAsync($"/{ip}?access_key={Configuration.GeoIpApiKey}");
        }
        
        private Task<HttpResponseMessage> LookUpIpScore(string ip, CancellationToken cancellationToken)
        {
            // API docs are available at https://getipintel.net/index.php#API.
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://check.getipintel.net");
            
            return client.GetAsync($"check.php?ip={ip}&contact={Configuration.GeoIpIntelEmailAddress}&flags=f", cancellationToken);
        }
    }
}