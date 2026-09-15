using System.ComponentModel.DataAnnotations;
using StudentHousing.Models;

namespace StudentHousing.ViewModels.Owner
{
    /// <summary>Create / edit form for a property. Rooms are edited as a list, images can be uploaded.</summary>
    public class PropertyFormViewModel
    {
        public int? Id { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        public PropertyType PropertyType { get; set; } = PropertyType.Apartment;

        [Required, StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string State { get; set; } = string.Empty;

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        [StringLength(150), Display(Name = "Nearest university")]
        public string? University { get; set; }

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }

        [Range(0, int.MaxValue)]
        public int Deposit { get; set; }

        [Range(1, 20)]
        public int Bedrooms { get; set; } = 1;

        [Range(0, 10)]
        public int Bathrooms { get; set; } = 1;

        public bool IsFurnished { get; set; }
        public bool PetAllowed { get; set; }

        [DataType(DataType.Date), Display(Name = "Available from")]
        public DateTime? AvailableFrom { get; set; }

        /// <summary>Rooms of the property (bound from the dynamic list in the form).</summary>
        public List<RoomFormViewModel> Rooms { get; set; } = new List<RoomFormViewModel>();

        /// <summary>Ids of amenities selected for this property.</summary>
        public List<int> SelectedAmenityIds { get; set; } = new List<int>();

        /// <summary>All active amenities, used to render checkboxes.</summary>
        public List<Amenity> AllAmenities { get; set; } = new List<Amenity>();

        /// <summary>Newly uploaded images (maximum 6).</summary>
        public List<IFormFile>? UploadedImages { get; set; }

        /// <summary>Existing images so they can be shown while editing.</summary>
        public List<PropertyImage> ExistingImages { get; set; } = new List<PropertyImage>();

        public string ApprovalStatusDisplay { get; set; } = string.Empty;
    }
}
