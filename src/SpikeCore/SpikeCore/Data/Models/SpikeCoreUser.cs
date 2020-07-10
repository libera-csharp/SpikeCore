using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace SpikeCore.Data.Models
{
    public class SpikeCoreUser : IdentityUser
    {
        [NotMapped] public IEnumerable<string> Roles { get; set; }

        public bool IsAdmin()
        {
            return Roles.Any(x => x.Equals("Admin", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
