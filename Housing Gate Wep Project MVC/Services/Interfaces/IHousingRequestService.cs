using StudentHousing.Models;
using StudentHousing.ViewModels;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Interfaces
{
    public interface IHousingRequestService
    {
        Task<PaginatedResult<HousingRequestCardViewModel>> SearchAsync(string? city, string? university, int? budget, TenantGender? gender, int page, int pageSize);
        Task<HousingRequest?> GetByIdAsync(int id);
        Task<HousingRequest?> GetByIdWithOffersAsync(int id);
        Task<(bool Success, string Error, int? Id)> CreateAsync(HousingRequestFormViewModel model, int studentProfileId);
        Task<(bool Success, string Error)> UpdateAsync(HousingRequestFormViewModel model, int studentProfileId);
        Task<(bool Success, string Error)> DeleteAsync(int id, int studentProfileId);
        Task<(bool Success, string Error)> ToggleCloseAsync(int id, int studentProfileId);
        Task<(bool Success, string Error)> AddOfferAsync(int requestId, string offererUserId, HousingRequestOfferFormViewModel model);
        Task<IReadOnlyList<HousingRequest>> GetMyRequestsAsync(int studentProfileId);
    }
}
