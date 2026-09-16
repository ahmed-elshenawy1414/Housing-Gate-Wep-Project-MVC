using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>
    /// An individual bed within a room. Beds can eventually be tied to a
    /// student profile so occupancy is tracked per bed.
    /// </summary>
    public class Bed
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        /// <summary>Student currently occupying this bed, if any.</summary>
        public int? StudentProfileId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = new byte[8];

        public Room Room { get; set; } = null!;
        public StudentProfile? Student { get; set; }
    }
}



