using StudentHousing.Models;
using StudentHousing.ViewModels;

namespace StudentHousing.Repositories.Interfaces
{
    public interface IHousingRequestRepository : IRepository<HousingRequest>
    {
        Task<HousingRequest?> GetByIdWithDetailsAsync(int id);
        Task<PaginatedResult<ViewModels.Student.HousingRequestCardViewModel>> SearchCardsAsync(string? city, string? university, int? budgetMin, int? budgetMax, TenantGender? gender, int page, int pageSize);
        Task<IReadOnlyList<HousingRequest>> GetByStudentProfileAsync(int studentProfileId);
        Task<IReadOnlyList<HousingRequestOffer>> GetOffersForRequestAsync(int requestId);
    }
}
