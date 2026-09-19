using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHousing.Models
{
    public class HousingRequest
    {
        public int Id { get; set; }

        [Required]
        public int StudentProfileId { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(60)]
        public string? Governorate { get; set; }

        [StringLength(150)]
        public string? University { get; set; }

        public PropertyType PropertyType { get; set; } = PropertyType.Apartment;

        [Range(0, int.MaxValue)]
        public int BudgetMin { get; set; }

        [Range(0, int.MaxValue)]
        public int BudgetMax { get; set; }

        [Range(1, 20)]
        public int Bedrooms { get; set; } = 1;

        [Range(0, 10)]
        public int Bathrooms { get; set; } = 1;

        public bool IsFurnished { get; set; }
        public bool PetAllowed { get; set; }

        public TenantGender PreferredGender { get; set; } = TenantGender.Any;

        public DateTime? MoveInDate { get; set; }

        [Range(1, 24)]
        public int MinLeaseMonths { get; set; } = 1;

        public bool IsActive { get; set; } = true;
        public bool IsClosed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = new byte[8];

        public StudentProfile StudentProfile { get; set; } = null!;
        public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
        public ICollection<HousingRequestOffer> Offers { get; set; } = new List<HousingRequestOffer>();

        [NotMapped]
        public int MinBudget => Math.Min(BudgetMin, BudgetMax);
        [NotMapped]
        public int MaxBudget => Math.Max(BudgetMin, BudgetMax);
    }

    public class HousingRequestOffer
    {
        public int Id { get; set; }

        [Required]
        public int HousingRequestId { get; set; }

        [Required]
        public string OffererUserId { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Message { get; set; }

        public int? OfferedPropertyId { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = new byte[8];

        public HousingRequest HousingRequest { get; set; } = null!;
        public ApplicationUser Offerer { get; set; } = null!;
        public Property? OfferedProperty { get; set; }
    }
}
