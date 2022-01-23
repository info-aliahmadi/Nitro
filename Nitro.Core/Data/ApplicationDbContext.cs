
using Microsoft.EntityFrameworkCore;
using Nitro.Core.Data.Domain;
using Nitro.Core.Data.EntityTypeConfigs;
using Nitro.Infrastructure;
using Nitro.Infrastructure.Data.IdentityDomain;

namespace Nitro.Core.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationDbContext : InfraDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new AuthorConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new ContentConfiguration());

        }
        public DbSet<Author> Author { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Content> Content { get; set; }

    }
}
