using System;
using Microsoft.AspNetCore.Identity;

namespace SpikeCore.Web.TokenProviders
{
    public class PasswordlessLoginTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public PasswordlessLoginTokenProviderOptions()
        {
            Name = "PasswordlessLoginProvider";
            TokenLifespan = TimeSpan.FromMinutes(10);
        }
    }
}