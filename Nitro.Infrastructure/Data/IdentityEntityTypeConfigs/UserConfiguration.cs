using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nitro.Infrastructure.Data.IdentityDomain;

namespace Nitro.Infrastructure.Data.IdentityEntityTypeConfigs
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User","Auth");

        }
    }
}
