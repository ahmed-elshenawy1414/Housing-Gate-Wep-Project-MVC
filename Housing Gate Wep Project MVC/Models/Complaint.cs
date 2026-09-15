using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>A complaint raised by a user (student or owner), handled by admins.</summary>
    public class Complaint
    {
        public int Id { get; set; }

        [Required]
        public string ComplainantId { get; set; } = string.Empty;

        public string? TargetUserId { get; set; }

        public int? TargetPropertyId { get; set; }

        public ComplaintType ComplaintType { get; set; } = ComplaintType.Other;

        [Required, StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;

        [StringLength(1000)]
        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        public ApplicationUser Complainant { get; set; } = null!;
        public ApplicationUser? TargetUser { get; set; }
        public Property? TargetProperty { get; set; }
    }
}
