using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudentHousing.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(60)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(60)]
        public string LastName { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        public string? FacebookUrl { get; set; }

        public string? InstagramUrl { get; set; }

        public string? TikTokUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public OwnerProfile? OwnerProfile { get; set; }
        public StudentProfile? StudentProfile { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
