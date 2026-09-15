using StudentHousing.Models;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Interfaces
{
    public interface IReviewService
    {
        // --- Property reviews ---

        Task<IReadOnlyList<PropertyReview>> GetApprovedForPropertyAsync(int propertyId);

        Task<IReadOnlyList<PropertyReview>> GetByReviewerAsync(string reviewerUserId);

        /// <summary>Submits a property review after a verified stay.</summary>
        Task<(bool Success, string Error)> AddPropertyReviewAsync(PropertyReviewFormViewModel model, string reviewerUserId);

        // --- Roommate reviews ---

        Task<IReadOnlyList<ApplicationUser>> GetRoommatesAsync(int stayId, string currentUserId);

        Task<(bool Success, string Error)> AddUserReviewAsync(int stayId, string reviewedUserId, string reviewerId, int rating, string comment);

        // --- Moderation ---

        Task<IReadOnlyList<PropertyReview>> GetPendingAsync();

        Task<(bool Success, string Error)> ModerateAsync(int reviewId, bool approve);
    }
}
