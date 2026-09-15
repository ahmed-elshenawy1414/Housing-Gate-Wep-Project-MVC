using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    public class StudentProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string University { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Major { get; set; }

        [StringLength(60)]
        public string? Governorate { get; set; }

        [StringLength(60)]
        public string? District { get; set; }

        [Range(1, 10)]
        public int? AcademicYear { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public Gender Gender { get; set; } = Gender.PreferNotToSay;

        [StringLength(800)]
        public string? Bio { get; set; }

        // --- Housing budget / timing ---
        [Range(0, int.MaxValue)]
        public int BudgetMin { get; set; }

        [Range(0, int.MaxValue)]
        public int BudgetMax { get; set; }

        public DateTime? MoveInDate { get; set; }

        [Range(1, 24)]
        public int MinLeaseMonths { get; set; } = 1;

        // --- Lifestyle (used by the matching engine) ---
        public bool IsSmoker { get; set; }
        public SleepSchedule SleepSchedule { get; set; } = SleepSchedule.Flexible;
        public NoiseLevel NoiseLevel { get; set; } = NoiseLevel.Moderate;
        public Cleanliness Cleanliness { get; set; } = Cleanliness.Casual;
        public bool HasPets { get; set; }

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;

        /// <summary>File path of the uploaded National ID image. Sensitive - admin access only.</summary>
        public string? NationalIdDocumentUrl { get; set; }

        /// <summary>File path of the uploaded university ID / student card image. Sensitive - admin access only.</summary>
        public string? UniversityIdDocumentUrl { get; set; }

        public string? VerificationRejectReason { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; } = null!;
        public RoommatePreference? Preference { get; set; }
        public ICollection<PropertyApplication> Applications { get; set; } = new List<PropertyApplication>();
        public ICollection<Stay> Stays { get; set; } = new List<Stay>();

        public int VerifiedStayCount { get; set; }
    }
}
