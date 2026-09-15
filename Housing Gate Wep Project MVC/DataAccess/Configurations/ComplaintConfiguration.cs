using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
    {
        public void Configure(EntityTypeBuilder<Complaint> builder)
        {
            builder.Property(c => c.Subject).HasMaxLength(150).IsRequired();
            builder.Property(c => c.Description).HasMaxLength(2000).IsRequired();

            builder.HasIndex(c => c.Status);

            builder.HasOne(c => c.Complainant)
                   .WithMany()
                   .HasForeignKey(c => c.ComplainantId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.TargetUser)
                   .WithMany()
                   .HasForeignKey(c => c.TargetUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
