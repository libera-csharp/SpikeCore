using Microsoft.AspNetCore.Identity;

namespace SpikeCore.Web.TokenProviders
{
    public static class CustomIdentityBuilderExtensions
    {
        public static IdentityBuilder AddPasswordlessLoginTokenProvider(this IdentityBuilder builder)
        {
            return builder.AddTokenProvider(nameof(PasswordlessLoginTokenProvider),
                typeof(PasswordlessLoginTokenProvider));
        }
    }
}