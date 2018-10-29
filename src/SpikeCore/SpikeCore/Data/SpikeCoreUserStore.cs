using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SpikeCore.Data.Models;

namespace SpikeCore.Data
{
    public class SpikeCoreUserStore : UserStore<SpikeCoreUser, IdentityRole, SpikeCoreDbContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, SpikeCoreUserLogin, IdentityUserToken<string>, IdentityRoleClaim<string>>
    {
        public SpikeCoreUserStore(SpikeCoreDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer) { }

        // HACK - Ideally this would be a separate method called something like FindByHostMaskAsync
        // Unfortunately I couldn't get that exposed in a way that worked with asp.net core identity.
        // So this is the resulting hack...
        public override async Task<SpikeCoreUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            if (loginProvider.Equals("IrcHost"))
            {
                var directMatchUser = await base.FindByLoginAsync(loginProvider, providerKey, cancellationToken);

                if (directMatchUser != null)
                    return directMatchUser;

                var startsWithLogin = await base.Context
                    .Set<SpikeCoreUserLogin>()
                    .Where(ul => ul.LoginProvider == loginProvider && ul.MatchType == "StartsWith" && providerKey.StartsWith(ul.ProviderKey))
                    .FirstOrDefaultAsync(cancellationToken);

                if (startsWithLogin != null)
                {
                    var startsWithUser = await base.Context
                        .Set<SpikeCoreUser>()
                        .SingleAsync(u => u.Id.Equals(startsWithLogin.UserId), cancellationToken);

                    if (startsWithUser != null)
                        return startsWithUser;
                }

                return null;
            }
            else
            {
                return await base.FindByLoginAsync(loginProvider, providerKey, cancellationToken);
            }
        }
    }
}
