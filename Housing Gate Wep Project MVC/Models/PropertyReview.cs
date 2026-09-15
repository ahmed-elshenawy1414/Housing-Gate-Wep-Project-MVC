using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>
    /// A review of a property, only possible after a verified stay.
    /// Moderated by admins before it is shown publicly.
    /// </summary>
    public class PropertyReview
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }

        public int StayId { get; set; }

        [Required]
        public string ReviewerId { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public ReviewStatus Status { get; set; } = ReviewStatus.Approved;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Property Property { get; set; } = null!;
        public Stay Stay { get; set; } = null!;
        public ApplicationUser Reviewer { get; set; } = null!;
    }
}
