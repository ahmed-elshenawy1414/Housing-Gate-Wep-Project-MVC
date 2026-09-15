using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class OwnerProfileConfiguration : IEntityTypeConfiguration<OwnerProfile>
    {
        public void Configure(EntityTypeBuilder<OwnerProfile> builder)
        {
            builder.HasMany(o => o.Properties)
                   .WithOne(p => p.Owner)
                   .HasForeignKey(p => p.OwnerId)
                   .HasPrincipalKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
