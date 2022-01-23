using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nitro.Infrastructure.Data.IdentityDomain;

namespace Nitro.Infrastructure.Data.IdentityEntityTypeConfigs
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRole", "Auth");
        }
    }
}
