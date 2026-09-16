using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class PropertyReviewImageConfiguration : IEntityTypeConfiguration<PropertyReviewImage>
    {
        public void Configure(EntityTypeBuilder<PropertyReviewImage> builder)
        {
            builder.Property(i => i.FilePath).HasMaxLength(300).IsRequired();
            builder.HasIndex(i => i.PropertyReviewId);
        }
    }
}
