using System.ComponentModel.DataAnnotations;
using StudentHousing.Models;

namespace StudentHousing.ViewModels.Owner
{
    public class OwnerProfileViewModel
    {
        [StringLength(120)]
        public string? CompanyName { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(60)]
        public string? Phone { get; set; }

        [StringLength(100), Display(Name = "Ownership / license number")]
        public string? LicenseNumber { get; set; }

        public VerificationStatus VerificationStatus { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool HasVerificationDocument { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }
}
