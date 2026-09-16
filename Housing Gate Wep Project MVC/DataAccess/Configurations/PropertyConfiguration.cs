using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class PropertyConfiguration : IEntityTypeConfiguration<Property>
    {
        public void Configure(EntityTypeBuilder<Property> builder)
        {
            builder.Property(p => p.Title).HasMaxLength(150).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(2000).IsRequired();
            builder.Property(p => p.Address).HasMaxLength(200).IsRequired();
            builder.Property(p => p.City).HasMaxLength(100).IsRequired();
            builder.Property(p => p.State).HasMaxLength(50).IsRequired();
            builder.Property(p => p.PublicId).HasMaxLength(20);
            builder.Property(p => p.District).HasMaxLength(100);
            builder.Property(p => p.University).HasMaxLength(150);

            builder.Property(p => p.RowVersion).IsRowVersion();
            builder.HasIndex(p => new { p.ApprovalStatus, p.IsActive });
            builder.HasIndex(p => p.City);
            builder.HasIndex(p => p.PublicId).IsUnique().HasFilter("[PublicId] IS NOT NULL");

            builder.HasMany(p => p.Images)
                   .WithOne(i => i.Property)
                   .HasForeignKey(i => i.PropertyId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Rooms)
                   .WithOne(r => r.Property)
                   .HasForeignKey(r => r.PropertyId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Reviews)
                   .WithOne(r => r.Property)
                   .HasForeignKey(r => r.PropertyId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Amenities)
                   .WithMany(a => a.Properties)
                   .UsingEntity("PropertyAmenities");

            // Do not delete a property that is referenced by a complaint.
            builder.HasMany<Complaint>()
                   .WithOne(c => c.TargetProperty)
                   .HasForeignKey(c => c.TargetPropertyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
