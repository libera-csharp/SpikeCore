using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpikeCore.Data.Models;

namespace SpikeCore.Web.TokenProviders
{
    /// <summary>
    /// A <see cref="DataProtectorTokenProvider{TUser}"/> implementation that we can use to hand out tokens via IRC,
    /// and then allow web users to redeem them for authentication. This class is specialized to work with <see cref="SpikeCoreUser"/>.
    ///
    /// This is based an article at https://andrewlock.net/implementing-custom-token-providers-for-passwordless-authentication-in-asp-net-core-identity/.
    /// </summary>
    public class PasswordlessLoginTokenProvider : DataProtectorTokenProvider<SpikeCoreUser>
    {
        public const string AuthType = "passwordless-auth";

        public PasswordlessLoginTokenProvider(IDataProtectionProvider dataProtectionProvider,
            IOptions<DataProtectionTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<SpikeCoreUser>> logger) :
            base(dataProtectionProvider, options, logger)
        {
        }
    }
}