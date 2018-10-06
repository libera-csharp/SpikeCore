using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SpikeCore.Data.Models;

namespace SpikeCore.Data
{
    public class SpikeCoreDbContext : IdentityDbContext<SpikeCoreUser>
    {
        public DbSet<KarmaItem> Karma { get; set; }
        
        public SpikeCoreDbContext(DbContextOptions<SpikeCoreDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<KarmaItem>()
                .HasIndex(karmaItem => karmaItem.Id)
                .IsUnique();

            builder.Entity<KarmaItem>()
                .HasIndex(karmaItem => karmaItem.Name)
                .IsUnique();
        }
    }
}
