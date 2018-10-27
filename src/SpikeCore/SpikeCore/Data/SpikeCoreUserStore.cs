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

        // HACK - Ideally this would be a seperate method called something like FindByHostMaskAsync
        // Unfortunatly I couldn't get that exposed in a way that worked with asp.net core identity.
        // So this is the resulting hack...
        public async override Task<SpikeCoreUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            if (loginProvider.Equals("IrcHost"))
            {
                var directMatchUser = await base.FindByLoginAsync(loginProvider, providerKey, cancellationToken);

                if (directMatchUser != null)
                    return directMatchUser;

                var startsWithLogin = await base.Context
                    .Set<SpikeCoreUserLogin>()
                    .Where(ul => ul.LoginProvider == loginProvider && ul.MetaData == "StartsWith" && providerKey.StartsWith(ul.ProviderKey))
                    .FirstOrDefaultAsync();

                if (startsWithLogin != null)
                {
                    var startsWithUser = await base.Context
                        .Set<SpikeCoreUser>()
                        .SingleAsync(u => u.Id.Equals(startsWithLogin.UserId));

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
