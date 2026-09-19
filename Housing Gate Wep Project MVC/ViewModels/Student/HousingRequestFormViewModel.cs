using System.ComponentModel.DataAnnotations;
using StudentHousing.Models;

namespace StudentHousing.ViewModels.Student
{
    public class HousingRequestFormViewModel
    {
        public int? Id { get; set; }

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

        [DataType(DataType.Date)]
        public DateTime? MoveInDate { get; set; }

        [Range(1, 24)]
        public int MinLeaseMonths { get; set; } = 1;

        public List<int> SelectedAmenityIds { get; set; } = new();
        public List<Amenity> AllAmenities { get; set; } = new();
    }

    public class HousingRequestCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? University { get; set; }
        public PropertyType PropertyType { get; set; }
        public int BudgetMin { get; set; }
        public int BudgetMax { get; set; }
        public int Bedrooms { get; set; }
        public TenantGender PreferredGender { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public bool StudentIsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OffersCount { get; set; }
    }

    public class HousingRequestOfferFormViewModel
    {
        [Required, StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        public int? OfferedPropertyId { get; set; }
    }
}
