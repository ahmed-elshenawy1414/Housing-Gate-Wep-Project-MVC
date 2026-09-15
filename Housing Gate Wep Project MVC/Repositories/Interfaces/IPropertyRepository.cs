using StudentHousing.Models;

namespace StudentHousing.Repositories.Interfaces
{
    public interface IPropertyRepository : IRepository<Property>
    {
        Task<Property?> GetByIdWithDetailsAsync(int id);

        Task<Property?> GetAdminDetailsAsync(int id);

        /// <summary>Approved + active listings, newest first, with owner, images and reviews.</summary>
        Task<IReadOnlyList<Property>> GetApprovedActiveAsync();

        Task<IReadOnlyList<Property>> GetByOwnerAsync(string ownerId);

        Task<int> CountByStatusAsync(ApprovalStatus status);

        Task<IReadOnlyList<Property>> SearchAsync(ApprovalStatus? status, string? search);
    }
}
