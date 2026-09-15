using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class PropertyApplicationConfiguration : IEntityTypeConfiguration<PropertyApplication>
    {
        public void Configure(EntityTypeBuilder<PropertyApplication> builder)
        {
            builder.Property(a => a.Message).HasMaxLength(1000);
            builder.HasIndex(a => new { a.RoomId, a.Status });
            builder.HasIndex(a => a.StudentProfileId);

            // One application may create at most one stay.
            builder.HasOne(a => a.Stay)
                   .WithOne(s => s.Application)
                   .HasForeignKey<Stay>(s => s.ApplicationId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
