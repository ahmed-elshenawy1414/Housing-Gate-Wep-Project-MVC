using StudentHousing.DTOs;

namespace StudentHousing.ViewModels.Student
{
    public class MatchesViewModel
    {
        /// <summary>True when the student has not completed their profile or preferences yet.</summary>
        public bool ProfileIncomplete { get; set; }
        public List<RoommateMatchDto> Matches { get; set; } = new List<RoommateMatchDto>();
    }
}
