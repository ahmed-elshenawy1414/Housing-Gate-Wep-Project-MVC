using StudentHousing.DTOs;

namespace StudentHousing.Services.Interfaces
{
    /// <summary>
    /// The roommate matching engine. Compares a student's profile + preferences
    /// against every other student and returns ranked compatibility scores.
    /// </summary>
    public interface IMatchingService
    {
        Task<IReadOnlyList<RoommateMatchDto>> GetMatchesAsync(int myStudentProfileId);

        /// <summary>
        /// Core scoring logic (0 - 100). Exposed so tests can call it directly.
        /// Uses the first profile's preferences as the "wants" when available.
        /// </summary>
        int Score(StudentHousing.Models.StudentProfile a, StudentHousing.Models.StudentProfile b);
    }
}
