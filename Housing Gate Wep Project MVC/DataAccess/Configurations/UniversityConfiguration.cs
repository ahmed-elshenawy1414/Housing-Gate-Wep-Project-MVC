using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class UniversityConfiguration : IEntityTypeConfiguration<University>
    {
        public void Configure(EntityTypeBuilder<University> builder)
        {
            builder.Property(u => u.Name).HasMaxLength(150).IsRequired();
            builder.Property(u => u.City).HasMaxLength(50);

            builder.HasIndex(u => u.Name).IsUnique();
        }
    }
}
