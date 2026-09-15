using System.ComponentModel.DataAnnotations;
using StudentHousing.Models;

namespace StudentHousing.ViewModels.Student
{
    /// <summary>What a student is looking for in a roommate (feeds the matching engine).</summary>
    public class RoommatePreferenceViewModel
    {
        [Display(Name = "Preferred gender")]
        public Gender? PreferredGender { get; set; }

        [Range(0, int.MaxValue), Display(Name = "Roommate budget (min)")]
        public int MinBudget { get; set; }

        [Range(0, int.MaxValue), Display(Name = "Roommate budget (max)")]
        public int MaxBudget { get; set; }

        [Display(Name = "Accept smokers")]
        public bool AcceptsSmokers { get; set; }

        [Display(Name = "Preferred sleep schedule")]
        public SleepSchedule? PreferredSleepSchedule { get; set; }

        [Display(Name = "Preferred noise level")]
        public NoiseLevel? PreferredNoiseLevel { get; set; }

        [Display(Name = "Preferred cleanliness")]
        public Cleanliness? PreferredCleanliness { get; set; }

        [Display(Name = "Accept roommates with pets")]
        public bool AcceptsPets { get; set; }

        [StringLength(800)]
        public string? Notes { get; set; }
    }
}
