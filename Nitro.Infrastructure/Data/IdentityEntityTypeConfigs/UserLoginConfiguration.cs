using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nitro.Infrastructure.Data.IdentityDomain;

namespace Nitro.Infrastructure.Data.IdentityEntityTypeConfigs
{
    public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
    {
        public void Configure(EntityTypeBuilder<UserLogin> builder)
        {
            builder.ToTable("UserLogin", "Auth");
        }
    }
}
