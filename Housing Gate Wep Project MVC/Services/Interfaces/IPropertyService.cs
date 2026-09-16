using StudentHousing.Models;
using StudentHousing.ViewModels;
using StudentHousing.ViewModels.Owner;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Interfaces
{
    public interface IPropertyService
    {
        // --- Public search / reading ---

        Task<IReadOnlyList<PropertyCardViewModel>> GetForHomeAsync(int take = 6);

        Task<IReadOnlyList<PropertyCardViewModel>> SearchAsync(PropertySearchViewModel search);

        Task<PaginatedResult<PropertyCardViewModel>> SearchPagedAsync(PropertySearchViewModel search);

        Task<Property?> GetByIdWithDetailsAsync(int id);

        Task<IReadOnlyList<string>> GetCitiesAsync();

        Task<int> CountApprovedActiveAsync();

        /// <summary>All active amenities, for property / room forms.</summary>
        Task<IReadOnlyList<Amenity>> GetActiveAmenitiesAsync();

        // --- Owner management ---

        Task<IReadOnlyList<Property>> GetByOwnerAsync(string ownerId);

        Task<(bool Success, string Error)> CreateAsync(PropertyFormViewModel model, string ownerId);

        Task<(bool Success, string Error)> UpdateAsync(PropertyFormViewModel model, string ownerId);

        Task<(bool Success, string Error)> ToggleActiveAsync(int propertyId, string ownerId);

        Task<(bool Success, string Error)> DeleteAsync(int propertyId, string ownerId);

        Task<(bool Success, string Error)> DeleteImageAsync(int propertyId, int imageId, string ownerId);

        // --- Applications ---

        Task<IReadOnlyList<PropertyApplication>> GetApplicationsForOwnerAsync(string ownerId);

        Task<(bool Success, string Error)> ApplyAsync(int roomId, int studentProfileId, string? message);

        Task<(bool Success, string Error)> RespondToApplicationAsync(int applicationId, string ownerId, bool approve);

        /// <summary>Count of pending applications for an owner (badge / dashboard).</summary>
        Task<int> CountPendingApplicationsAsync(string ownerId);
    }
}
