using System;
using Microsoft.AspNetCore.Identity;

namespace SpikeCore.Web.TokenProviders
{
    public class PasswordlessLoginTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public PasswordlessLoginTokenProviderOptions()
        {
            Name = nameof(PasswordlessLoginTokenProvider);
            TokenLifespan = TimeSpan.FromMinutes(10);
        }
    }
}