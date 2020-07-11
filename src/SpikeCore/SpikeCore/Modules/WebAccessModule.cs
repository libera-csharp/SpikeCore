using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Identity;
using SpikeCore.Data.Models;
using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    public class WebAccessModule : ModuleBase
    {
        public override string Name => "WebAccess";
        public override string Description => "Provides access to the bot via a web UI";
        public override string Instructions => "webaccess";

        private readonly UserManager<SpikeCoreUser> _userManager;

        public WebAccessModule(UserManager<SpikeCoreUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleMessageAsyncInternal(IrcPrivMessage request,
            CancellationToken cancellationToken)
        {
            var token = HttpUtility.UrlEncode(await _userManager.GenerateUserTokenAsync(request.IdentityUser, "PasswordlessLoginProvider",
                "passwordless-auth"));

            await SendMessageToNick(request.UserName,
                $"{Configuration.WebAccessHost}/Authentication/TokenCallback?email={request.IdentityUser.Email}&token={token}");
        }
    }
}