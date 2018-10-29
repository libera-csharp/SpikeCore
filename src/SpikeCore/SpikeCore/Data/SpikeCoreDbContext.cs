using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SpikeCore.Data.Models;

namespace SpikeCore.Data
{
    public class SpikeCoreDbContext : IdentityDbContext<SpikeCoreUser, IdentityRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, SpikeCoreUserLogin, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public DbSet<KarmaItem> Karma { get; set; }
        public DbSet<Factoid> Factoids { get; set; }
        
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
            
            builder.Entity<Factoid>(b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd();

                b.HasKey(factoid => factoid.Id);

                b.HasIndex(factoid => factoid.Name);
            });
        }
    }
}
