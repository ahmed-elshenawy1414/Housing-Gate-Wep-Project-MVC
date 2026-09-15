using StudentHousing.Models;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Interfaces
{
    public interface IStudentService
    {
        Task<StudentProfile?> GetMyProfileAsync(string userId);

        Task<(bool Success, string Error)> UpdateProfileAsync(string userId, StudentProfileViewModel model);

        Task<(bool Success, string Error)> UpdatePreferenceAsync(string userId, RoommatePreferenceViewModel model);

        /// <summary>Uploads a student ID document and sets verification status to Pending.</summary>
        Task<(bool Success, string Error)> SubmitVerificationAsync(string userId, IFormFile nationalId, IFormFile universityId);

        /// <summary>Completed stays for which the student has not written a property review yet.</summary>
        Task<IReadOnlyList<Stay>> GetCompletedStaysWithoutReviewAsync(int studentProfileId);
    }
}
