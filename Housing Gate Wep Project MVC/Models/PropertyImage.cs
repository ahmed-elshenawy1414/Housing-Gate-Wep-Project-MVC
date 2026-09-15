namespace StudentHousing.Models
{
    public class PropertyImage
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }

        /// <summary>Optional room this image belongs to (e.g. photos of a specific room).</summary>
        public int? RoomId { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public PropertyImageCategory Category { get; set; } = PropertyImageCategory.Other;

        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Property Property { get; set; } = null!;
        public Room? Room { get; set; }
    }
}
