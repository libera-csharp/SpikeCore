using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SpikeCore.Data
{
    public class SpikeCoreDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SpikeCoreDbContext>
    {
        public SpikeCoreDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SpikeCoreDbContext>();
            optionsBuilder.UseSqlite("DataSource=../SpikeCore.Web/DB/SpikeCore.db");

            return new SpikeCoreDbContext(optionsBuilder.Options);
        }
    }
}
