using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.Property(a => a.Action).HasMaxLength(60).IsRequired();

            builder.HasIndex(a => a.CreatedAt);

            builder.HasIndex(a => a.Action);

            builder.HasIndex(a => a.EntityType);

            builder.HasOne(a => a.PerformedBy)
                .WithMany()
                .HasForeignKey(a => a.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
