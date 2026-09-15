using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
    {
        public void Configure(EntityTypeBuilder<Amenity> builder)
        {
            builder.Property(a => a.Name).HasMaxLength(60).IsRequired();
            builder.Property(a => a.Category).HasMaxLength(30);

            builder.HasIndex(a => a.Name).IsUnique();
        }
    }
}
