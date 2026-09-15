using StudentHousing.Models;

namespace StudentHousing.ViewModels.Student
{
    public class PropertyDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PropertyType PropertyType { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? ZipCode { get; set; }
        public int Deposit { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public bool IsFurnished { get; set; }
        public bool PetAllowed { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public bool IsFeatured { get; set; }

        public List<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<PropertyReview> Reviews { get; set; } = new List<PropertyReview>();
        public double AverageRating { get; set; }

        public string OwnerName { get; set; } = string.Empty;
        public bool OwnerIsVerified { get; set; }

        /// <summary>Id of the room the current student already applied for, if any.</summary>
        public int? AlreadyAppliedRoomId { get; set; }

        public string? AppliedMessage { get; set; }
        public int? AppliedStatus { get; set; }
    }
}
