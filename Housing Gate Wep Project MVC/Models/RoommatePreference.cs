using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>
    /// What a student is looking for in a future roommate.
    /// The matching engine compares this against other students' profiles.
    /// </summary>
    public class RoommatePreference
    {
        public int Id { get; set; }

        [Required]
        public int StudentProfileId { get; set; }

        public Gender? PreferredGender { get; set; }

        [Range(0, int.MaxValue)]
        public int MinBudget { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxBudget { get; set; }

        public bool AcceptsSmokers { get; set; }
        public SleepSchedule? PreferredSleepSchedule { get; set; }
        public NoiseLevel? PreferredNoiseLevel { get; set; }
        public Cleanliness? PreferredCleanliness { get; set; }
        public bool AcceptsPets { get; set; }

        [StringLength(800)]
        public string? Notes { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public StudentProfile StudentProfile { get; set; } = null!;
    }
}
