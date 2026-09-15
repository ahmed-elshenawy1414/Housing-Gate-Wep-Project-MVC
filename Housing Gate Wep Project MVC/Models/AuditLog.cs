using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>
    /// Immutable administrative audit trail. Every sensitive admin action is
    /// appended here so the platform can answer "who did what, when, and why".
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string PerformedByUserId { get; set; } = string.Empty;

        /// <summary>Stable action key, e.g. "Property.Approve". Localized in the UI.</summary>
        [Required, StringLength(60)]
        public string Action { get; set; } = string.Empty;

        [StringLength(60)]
        public string? EntityType { get; set; }

        [StringLength(50)]
        public string? EntityId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser PerformedBy { get; set; } = null!;
    }
}
