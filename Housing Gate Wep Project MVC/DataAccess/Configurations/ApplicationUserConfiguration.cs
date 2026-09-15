using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FirstName).HasMaxLength(60).IsRequired();
            builder.Property(u => u.LastName).HasMaxLength(60).IsRequired();

            // Profiles are intentionally Restrict-deleted: admins deactivate users
            // instead of removing their history.
            builder.HasOne(u => u.OwnerProfile)
                   .WithOne(o => o.User)
                   .HasForeignKey<OwnerProfile>(o => o.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.StudentProfile)
                   .WithOne(s => s.User)
                   .HasForeignKey<StudentProfile>(s => s.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
