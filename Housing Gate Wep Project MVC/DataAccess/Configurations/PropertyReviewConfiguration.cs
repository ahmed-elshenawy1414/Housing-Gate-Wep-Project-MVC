using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class PropertyReviewConfiguration : IEntityTypeConfiguration<PropertyReview>
    {
        public void Configure(EntityTypeBuilder<PropertyReview> builder)
        {
            builder.Property(r => r.Title).HasMaxLength(120).IsRequired();
            builder.Property(r => r.Comment).HasMaxLength(2000).IsRequired();

            // A student may review a property only once per verified stay.
            builder.HasIndex(r => new { r.StayId, r.ReviewerId }).IsUnique();
        }
    }
}
