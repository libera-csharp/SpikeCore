using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SpikeCore.Data.Models
{
    public class SpikeCoreUser : IdentityUser
    {
        [NotMapped] public IEnumerable<string> Roles { get; set; }
    }
}