using StudentHousing.Models;

namespace StudentHousing.Repositories.Interfaces
{
    public interface IReviewRepository : IRepository<PropertyReview>
    {
        Task<IReadOnlyList<PropertyReview>> GetApprovedForPropertyAsync(int propertyId);

        /// <summary>Reviews waiting for moderation.</summary>
        Task<IReadOnlyList<PropertyReview>> GetPendingAsync();

        Task<IReadOnlyList<PropertyReview>> GetByReviewerAsync(string reviewerUserId);

        Task<IReadOnlyList<PropertyReview>> GetByPropertyOwnerAsync(string ownerId);

        Task<PropertyReview?> GetByIdWithDetailsAsync(int id);

        Task<bool> AlreadyReviewedStayAsync(int stayId, string reviewerUserId);

        // --- Roommate reviews ---

        Task AddUserReviewAsync(UserReview review);

        Task<bool> UserReviewExistsAsync(int stayId, string reviewerId, string reviewedUserId);
    }
}
