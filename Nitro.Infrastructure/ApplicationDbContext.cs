
using Microsoft.EntityFrameworkCore;
using Nitro.Core.Data.Domain;
using Nitro.Core.Data.EntityConfig.Cms;
using Nitro.Core.Data.Mapping.Cms;
using Nitro.Infrastructure;

namespace Nitro.Core.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationDbContext : IdentityContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Cms Builder

            modelBuilder.ApplyConfiguration(new AuthorBuilder());
            modelBuilder.ApplyConfiguration(new CategoryBuilder());
            modelBuilder.ApplyConfiguration(new ContentBuilder());

            #endregion


        }


        #region Cms DbSet

        public DbSet<Author> Author { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Content> Content { get; set; }

        #endregion

    }
}
