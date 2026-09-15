using StudentHousing.Models;

namespace StudentHousing.ViewModels
{
    /// <summary>Compact representation of a listing used in cards and lists.</summary>
    public class PropertyCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public PropertyType PropertyType { get; set; }
        public int MinRent { get; set; }
        public int Bedrooms { get; set; }
        public bool IsFurnished { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public bool OwnerIsVerified { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
    }
}
