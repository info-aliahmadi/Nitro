using Microsoft.EntityFrameworkCore;
using Nitro.Infrastructure.Data;

namespace Nitro.Migrations
{

    public class MigrationContext : ApplicationDbContext
    {
        public MigrationContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Seed();
            base.OnModelCreating(modelBuilder);

        }
    }
}
