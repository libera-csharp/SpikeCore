using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SpikeCore.Web.TokenProviders
{
    /// <summary>
    /// A <see cref="DataProtectorTokenProvider{TUser}"/> implementation that we can use to hand out tokens via IRC,
    /// and then allow web users to redeem them for authentication.
    ///
    /// This is based an article at https://andrewlock.net/implementing-custom-token-providers-for-passwordless-authentication-in-asp-net-core-identity/.
    /// </summary>
    /// <typeparam name="TUser">The type used to represent a user.</typeparam>
    public class PasswordlessLoginTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public PasswordlessLoginTokenProvider(IDataProtectionProvider dataProtectionProvider,
            IOptions<DataProtectionTokenProviderOptions> options, ILogger<DataProtectorTokenProvider<TUser>> logger) :
            base(dataProtectionProvider, options, logger)
        {
        }
    }
}