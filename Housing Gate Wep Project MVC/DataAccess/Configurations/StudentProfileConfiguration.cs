using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
    {
        public void Configure(EntityTypeBuilder<StudentProfile> builder)
        {
            builder.Property(s => s.University).HasMaxLength(120).IsRequired();
            builder.Property(s => s.Governorate).HasMaxLength(60);
            builder.Property(s => s.District).HasMaxLength(60);
            builder.Property(s => s.VerificationRejectReason).HasMaxLength(500);
            builder.HasIndex(s => s.VerificationStatus);
            builder.HasIndex(s => s.UserId).IsUnique();

            builder.HasOne(s => s.Preference)
                   .WithOne(p => p.StudentProfile)
                   .HasForeignKey<RoommatePreference>(p => p.StudentProfileId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Applications)
                   .WithOne(a => a.StudentProfile)
                   .HasForeignKey(a => a.StudentProfileId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Stays)
                   .WithOne(st => st.StudentProfile)
                   .HasForeignKey(st => st.StudentProfileId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
