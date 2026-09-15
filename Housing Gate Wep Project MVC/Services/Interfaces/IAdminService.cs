using StudentHousing.DTOs;
using StudentHousing.Models;
using StudentHousing.ViewModels.Admin;

namespace StudentHousing.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();

        Task<IReadOnlyList<UserWithRolesDto>> GetAllUsersWithRolesAsync(string? search = null);

        Task<IReadOnlyList<ApplicationUser>> GetStudentsAsync(string? search = null);

        Task<IReadOnlyList<ApplicationUser>> GetOwnersAsync(string? search = null);

        Task<(bool Success, string Error)> VerifyStudentAsync(string userId, bool approve, string? reason = null);

        Task<(bool Success, string Error)> RequestStudentDocumentsAsync(string userId, string? reason);

        Task<(bool Success, string Error)> VerifyOwnerAsync(string userId, bool approve);

        Task<IReadOnlyList<Property>> GetPropertiesAsync(ApprovalStatus? status = null, string? search = null);

        Task<(bool Success, string Error)> ModeratePropertyAsync(int propertyId, bool approve, string? note);

        Task<(bool Success, string Error)> ToggleUserActiveAsync(string userId);

        Task<AdminUserDetailsViewModel?> GetUserDetailsAsync(string userId);

        Task<(bool Success, string Error)> DeleteUserAsync(string userId);

        Task<AdminPropertyDetailsViewModel?> GetPropertyDetailsAsync(int propertyId);

        Task<(bool Success, string Error)> DeletePropertyAsync(int propertyId);
    }
}
