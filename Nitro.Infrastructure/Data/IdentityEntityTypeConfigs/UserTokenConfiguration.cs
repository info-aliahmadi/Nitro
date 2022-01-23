using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nitro.Infrastructure.Data.IdentityDomain;

namespace Nitro.Infrastructure.Data.IdentityEntityTypeConfigs
{
    public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> builder)
        {
            builder.ToTable("UserToken", "Auth");
        }
    }
}
