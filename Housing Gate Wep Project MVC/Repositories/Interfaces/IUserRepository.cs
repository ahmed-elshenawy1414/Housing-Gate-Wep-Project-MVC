using StudentHousing.DTOs;
using StudentHousing.Models;

namespace StudentHousing.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUserWithProfileAsync(string userId);

        Task<IReadOnlyList<ApplicationUser>> GetRecentUsersAsync(int take);

        Task<IReadOnlyList<UserWithRolesDto>> GetAllUsersWithRolesAsync(string? search = null);

        Task<IReadOnlyList<ApplicationUser>> GetOwnersWithProfilesAsync(string? search = null);

        Task<IReadOnlyList<ApplicationUser>> GetStudentsWithProfilesAsync(string? search = null);

        Task<IReadOnlyList<UserReview>> GetReviewsAboutUserAsync(string userId);

        Task<IReadOnlyList<UserReview>> GetReviewsByUserAsync(string userId);
    }
}
