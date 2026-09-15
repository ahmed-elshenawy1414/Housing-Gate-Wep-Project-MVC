using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class UserReviewConfiguration : IEntityTypeConfiguration<UserReview>
    {
        public void Configure(EntityTypeBuilder<UserReview> builder)
        {
            builder.Property(r => r.Comment).HasMaxLength(2000).IsRequired();

            // A student may review the same roommate once per shared stay.
            builder.HasIndex(r => new { r.StayId, r.ReviewerId, r.ReviewedUserId }).IsUnique();

            // Two paths to AspNetUsers would create a multiple-cascade cycle in SQL Server.
            builder.HasOne(r => r.Reviewer)
                   .WithMany()
                   .HasForeignKey(r => r.ReviewerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ReviewedUser)
                   .WithMany()
                   .HasForeignKey(r => r.ReviewedUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
