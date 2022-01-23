using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nitro.Infrastructure.Data.IdentityDomain;

namespace Nitro.Infrastructure.Data.IdentityEntityTypeConfigs
{
    public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
    {
        public void Configure(EntityTypeBuilder<UserClaim> builder)
        {
            builder.ToTable("UserClaim", "Auth");
        }
    }
}
