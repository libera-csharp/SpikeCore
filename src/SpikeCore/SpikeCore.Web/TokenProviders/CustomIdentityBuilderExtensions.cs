using Microsoft.AspNetCore.Identity;

namespace SpikeCore.Web.TokenProviders
{
    public static class CustomIdentityBuilderExtensions
    {
        public static IdentityBuilder AddPasswordlessLoginTokenProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var provider= typeof(PasswordlessLoginTokenProvider<>).MakeGenericType(userType);

            return builder.AddTokenProvider("PasswordlessLoginProvider", provider);
        }
    }
}