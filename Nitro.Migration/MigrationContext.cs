using Microsoft.EntityFrameworkCore;
using Nitro.Core.Data;
using Nitro.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
