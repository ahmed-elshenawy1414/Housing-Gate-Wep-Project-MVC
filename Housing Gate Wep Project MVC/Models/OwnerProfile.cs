using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    public class OwnerProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [StringLength(120)]
        public string? CompanyName { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(60)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? LicenseNumber { get; set; }

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;

        /// <summary>File path of the uploaded proof of ownership document.</summary>
        public string? VerificationDocumentUrl { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; } = null!;
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
