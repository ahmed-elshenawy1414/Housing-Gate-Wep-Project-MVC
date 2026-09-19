using System.ComponentModel.DataAnnotations;
using StudentHousing.Models;

namespace StudentHousing.ViewModels.Student
{
    /// <summary>A student's own profile details, used for editing.</summary>
    public class StudentProfileViewModel
    {
        [Required, StringLength(120)]
        public string University { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Major { get; set; }

        [StringLength(60), Display(Name = "Governorate of origin")]
        public string? Governorate { get; set; }

        [StringLength(60), Display(Name = "City / district of origin")]
        public string? District { get; set; }

        [Range(1, 10), Display(Name = "Academic year")]
        public int? AcademicYear { get; set; }

        [DataType(DataType.Date), Display(Name = "Date of birth")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Please select your gender")]
        [Range(0, 1, ErrorMessage = "Please select Male or Female")]
        public Gender Gender { get; set; } = Gender.PreferNotToSay;

        [StringLength(800)]
        public string? Bio { get; set; }

        [Range(0, int.MaxValue), Display(Name = "Monthly budget (min)")]
        public int BudgetMin { get; set; }

        [Range(0, int.MaxValue), Display(Name = "Monthly budget (max)")]
        public int BudgetMax { get; set; }

        [DataType(DataType.Date), Display(Name = "Move-in date")]
        public DateTime? MoveInDate { get; set; }

        [Range(1, 24), Display(Name = "Minimum lease (months)")]
        public int MinLeaseMonths { get; set; } = 1;

        [Display(Name = "I smoke")]
        public bool IsSmoker { get; set; }

        [Display(Name = "Sleep schedule")]
        public SleepSchedule SleepSchedule { get; set; } = SleepSchedule.Flexible;

        [Display(Name = "Comfortable noise level")]
        public NoiseLevel NoiseLevel { get; set; } = NoiseLevel.Moderate;

        [Display(Name = "Cleanliness")]
        public Cleanliness Cleanliness { get; set; } = Cleanliness.Casual;

        [Display(Name = "I have pets")]
        public bool HasPets { get; set; }

        // Read-only fields shown on the page.
        public VerificationStatus VerificationStatus { get; set; }
        public int VerifiedStayCount { get; set; }
        public string? Email { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? VerificationRejectReason { get; set; }
        public bool HasNationalIdDocument { get; set; }
        public bool HasUniversityIdDocument { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
