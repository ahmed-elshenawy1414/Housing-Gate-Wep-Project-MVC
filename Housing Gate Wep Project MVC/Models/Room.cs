using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>
    /// An individual room within a property. Shared housing properties
    /// usually have several rooms, each with its own rent.
    /// </summary>
    public class Room
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public RoomType RoomType { get; set; } = RoomType.Single;

        [Range(0, int.MaxValue)]
        public int RentPerMonth { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public BathroomType BathroomType { get; set; } = BathroomType.Shared;

        [Range(0, 20)]
        public int NumberOfBeds { get; set; } = 1;

        [Range(0, 20)]
        public int AvailableBeds { get; set; } = 1;

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Property Property { get; set; } = null!;
        public ICollection<PropertyApplication> Applications { get; set; } = new List<PropertyApplication>();
        public ICollection<Stay> Stays { get; set; } = new List<Stay>();
        public ICollection<Bed> Beds { get; set; } = new List<Bed>();
        public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    }
}
