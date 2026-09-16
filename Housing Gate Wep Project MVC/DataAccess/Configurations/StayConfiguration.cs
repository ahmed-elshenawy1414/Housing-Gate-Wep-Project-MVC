using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class StayConfiguration : IEntityTypeConfiguration<Stay>
    {
        public void Configure(EntityTypeBuilder<Stay> builder)
        {
            builder.Property(s => s.RowVersion).IsRowVersion();
            builder.HasIndex(s => s.StudentProfileId);
            builder.HasIndex(s => new { s.RoomId, s.Status });
        }
    }
}
