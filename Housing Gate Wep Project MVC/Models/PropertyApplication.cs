using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>
    /// A student's request to move into a specific room.
    /// Once approved by the owner, a Stay record is created (a verified stay).
    /// </summary>
    public class PropertyApplication
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public int StudentProfileId { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        public Room Room { get; set; } = null!;
        public StudentProfile StudentProfile { get; set; } = null!;
        public Stay? Stay { get; set; }
    }
}
