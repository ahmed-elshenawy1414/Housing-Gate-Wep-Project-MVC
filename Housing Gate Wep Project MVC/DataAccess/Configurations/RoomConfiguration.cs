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
            builder.Property(r => r.RowVersion).IsRowVersion();

            builder.HasIndex(r => new { r.PropertyId, r.IsAvailable });

            // Data integrity: rent and bed counts must be non-negative
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Room_RentPerMonth_NonNegative", "[RentPerMonth] >= 0");
                t.HasCheckConstraint("CK_Room_NumberOfBeds_Range", "[NumberOfBeds] >= 0 AND [NumberOfBeds] <= 20");
                t.HasCheckConstraint("CK_Room_AvailableBeds_Range", "[AvailableBeds] >= 0 AND [AvailableBeds] <= [NumberOfBeds]");
            });

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
