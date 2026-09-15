using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>
    /// A structured amenity (Wi-Fi, Air Conditioning, ...) that admins manage.
    /// Amenities attach to properties and rooms via many-to-many joins.
    /// </summary>
    public class Amenity
    {
        public int Id { get; set; }

        [Required, StringLength(60)]
        public string Name { get; set; } = string.Empty;

        [StringLength(60)]
        public string? NameAr { get; set; }

        [StringLength(30)]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Property> Properties { get; set; } = new List<Property>();
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
