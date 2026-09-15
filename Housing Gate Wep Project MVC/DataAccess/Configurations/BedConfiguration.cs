using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class BedConfiguration : IEntityTypeConfiguration<Bed>
    {
        public void Configure(EntityTypeBuilder<Bed> builder)
        {
            builder.Property(b => b.Name).HasMaxLength(50).IsRequired();

            builder.HasIndex(b => new { b.RoomId, b.IsAvailable });

            builder.HasOne(b => b.Student)
                   .WithMany()
                   .HasForeignKey(b => b.StudentProfileId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
