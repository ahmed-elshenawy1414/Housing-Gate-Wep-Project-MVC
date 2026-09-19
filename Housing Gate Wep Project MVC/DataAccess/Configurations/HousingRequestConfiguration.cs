using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentHousing.Models;

namespace StudentHousing.Data.Configurations
{
    public class HousingRequestConfiguration : IEntityTypeConfiguration<HousingRequest>
    {
        public void Configure(EntityTypeBuilder<HousingRequest> builder)
        {
            builder.Property(r => r.Title).HasMaxLength(150).IsRequired();
            builder.Property(r => r.Description).HasMaxLength(2000).IsRequired();
            builder.Property(r => r.City).HasMaxLength(100).IsRequired();
            builder.Property(r => r.District).HasMaxLength(100);
            builder.Property(r => r.Governorate).HasMaxLength(60);
            builder.Property(r => r.University).HasMaxLength(150);
            builder.Property(r => r.RowVersion).IsRowVersion();

            builder.HasIndex(r => new { r.IsActive, r.IsClosed });
            builder.HasIndex(r => r.City);
            builder.HasIndex(r => r.StudentProfileId);
            builder.HasIndex(r => r.CreatedAt);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_HousingRequest_BudgetMin_NonNegative", "[BudgetMin] >= 0");
                t.HasCheckConstraint("CK_HousingRequest_BudgetMax_NonNegative", "[BudgetMax] >= 0");
                t.HasCheckConstraint("CK_HousingRequest_Bedrooms_Range", "[Bedrooms] >= 1 AND [Bedrooms] <= 20");
                t.HasCheckConstraint("CK_HousingRequest_Bathrooms_Range", "[Bathrooms] >= 0 AND [Bathrooms] <= 10");
            });

            builder.HasMany(r => r.Amenities).WithMany().UsingEntity("HousingRequestAmenities");
            builder.HasMany(r => r.Offers).WithOne(o => o.HousingRequest).HasForeignKey(o => o.HousingRequestId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(r => r.StudentProfile).WithMany().HasForeignKey(r => r.StudentProfileId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class HousingRequestOfferConfiguration : IEntityTypeConfiguration<HousingRequestOffer>
    {
        public void Configure(EntityTypeBuilder<HousingRequestOffer> builder)
        {
            builder.Property(o => o.Message).HasMaxLength(1000);
            builder.Property(o => o.RowVersion).IsRowVersion();
            builder.HasIndex(o => new { o.HousingRequestId, o.Status });
            builder.HasIndex(o => o.OffererUserId);
            builder.HasOne(o => o.Offerer).WithMany().HasForeignKey(o => o.OffererUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(o => o.OfferedProperty).WithMany().HasForeignKey(o => o.OfferedPropertyId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
