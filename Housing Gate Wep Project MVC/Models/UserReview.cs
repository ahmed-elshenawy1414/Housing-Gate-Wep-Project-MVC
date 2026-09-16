using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>
    /// A student review of another student they actually shared housing with
    /// (both must be connected through the same verified Stay).
    /// </summary>
    public class UserReview
    {
        public int Id { get; set; }

        public int StayId { get; set; }

        [Required]
        public string ReviewerId { get; set; } = string.Empty;

        [Required]
        public string ReviewedUserId { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required, StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = new byte[8];

        public Stay Stay { get; set; } = null!;
        public ApplicationUser Reviewer { get; set; } = null!;
        public ApplicationUser ReviewedUser { get; set; } = null!;
    }
}
