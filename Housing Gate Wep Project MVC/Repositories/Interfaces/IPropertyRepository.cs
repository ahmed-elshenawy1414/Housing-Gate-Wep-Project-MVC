using StudentHousing.Models;
using StudentHousing.ViewModels;
using StudentHousing.ViewModels.Student;

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

        // Phase 4: efficient server-side queries
        Task<PaginatedResult<PropertyCardViewModel>> SearchCardsAsync(PropertySearchViewModel search, int page, int pageSize);
        Task<IReadOnlyList<PropertyCardViewModel>> GetHomeCardsAsync(int take);
        Task<IReadOnlyList<string>> GetDistinctCitiesAsync();
        Task<int> CountApprovedActiveAsync();
    }
}
