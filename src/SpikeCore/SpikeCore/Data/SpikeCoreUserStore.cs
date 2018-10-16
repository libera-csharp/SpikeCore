using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SpikeCore.Data.Models;

namespace SpikeCore.Data
{
    public class SpikeCoreUserStore : UserStore<SpikeCoreUser, IdentityRole, SpikeCoreDbContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
    {
        public SpikeCoreUserStore(SpikeCoreDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer) { }

        // HACK - This should be a method with the name FindByLoginStartsWithAsync.
        // Unfortunatly I couldn't get that exposed in a way that worked with asp.net core identity.
        // So this is the resulting hack...
        public async override Task<SpikeCoreUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            if (loginProvider.Equals("IrcHostStartsWith"))
            {
                loginProvider = "IrcHost";

                var userLogins = base.Context
                    .Set<IdentityUserLogin<string>>()
                    .Where(ul => ul.LoginProvider == loginProvider && providerKey.StartsWith(ul.ProviderKey));

                //TODO Logic for prioritising which user to match

                var userLogin = await userLogins
                    .FirstOrDefaultAsync();

                var user = await base.Context
                    .Set<SpikeCoreUser>()
                    .SingleAsync(u => u.Id.Equals(userLogin.UserId));

                return user;
            }
            else
            {
                return await base.FindByLoginAsync(loginProvider, providerKey, cancellationToken);
            }
        }
    }
}
