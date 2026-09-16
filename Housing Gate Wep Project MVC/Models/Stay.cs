using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>
    /// A verified move-in. Created only after an owner approves an application,
    /// so it can be trusted as evidence of a real stay.
    /// </summary>
    public class Stay
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public int StudentProfileId { get; set; }

        public int ApplicationId { get; set; }

        public StayStatus Status { get; set; } = StayStatus.Active;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = new byte[8];

        public Room Room { get; set; } = null!;
        public StudentProfile StudentProfile { get; set; } = null!;
        public PropertyApplication Application { get; set; } = null!;
        public ICollection<PropertyReview> PropertyReviews { get; set; } = new List<PropertyReview>();
        public ICollection<UserReview> UserReviews { get; set; } = new List<UserReview>();
    }
}



