using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nitro.Core.Domain.Auth;
using Nitro.Core.Mapping.Auth;

namespace Nitro.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    public class IdentityContext : IdentityDbContext<User,Role,int,UserClaim,UserRole,UserLogin,RoleClaim,UserToken>
    {
        public IdentityContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            #region Auth Builder
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserClaimConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserLoginConfiguration());
            modelBuilder.ApplyConfiguration(new RoleClaimConfiguration());
            modelBuilder.ApplyConfiguration(new UserTokenConfiguration());
            #endregion

        }

        #region Auth DbSet
        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<UserClaim> UserClaim { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<UserLogin> UserLogin { get; set; }
        public DbSet<RoleClaim> RoleClaim { get; set; }
        public DbSet<UserToken> UserToken { get; set; }
        #endregion


    }
}
