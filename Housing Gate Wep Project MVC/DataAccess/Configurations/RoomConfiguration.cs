using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
            builder.Property(r => r.Description).HasMaxLength(500);

            builder.HasIndex(r => new { r.PropertyId, r.IsAvailable });

            // Room images stay attached to the property if the room is deleted.
            builder.HasMany(r => r.Images)
                   .WithOne(i => i.Room)
                   .HasForeignKey(i => i.RoomId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasMany(r => r.Beds)
                   .WithOne(b => b.Room)
                   .HasForeignKey(b => b.RoomId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Amenities)
                   .WithMany(a => a.Rooms)
                   .UsingEntity("RoomAmenities");

            // Rooms that already have applications / stays are protected from deletion.
            builder.HasMany(r => r.Applications)
                   .WithOne(a => a.Room)
                   .HasForeignKey(a => a.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.Stays)
                   .WithOne(s => s.Room)
                   .HasForeignKey(s => s.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
