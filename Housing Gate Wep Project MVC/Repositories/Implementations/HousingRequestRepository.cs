using Microsoft.EntityFrameworkCore;
using StudentHousing.Data;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.ViewModels;

namespace StudentHousing.Repositories.Implementations
{
    public class HousingRequestRepository : Repository<HousingRequest>, IHousingRequestRepository
    {
        public HousingRequestRepository(ApplicationDbContext db) : base(db) { }

        public async Task<HousingRequest?> GetByIdWithDetailsAsync(int id)
        {
            return await _db.HousingRequests
                .Include(r => r.StudentProfile.User)
                .Include(r => r.Amenities)
                .Include(r => r.Offers).ThenInclude(o => o.Offerer)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<ViewModels.PaginatedResult<ViewModels.Student.HousingRequestCardViewModel>> SearchCardsAsync(string? city, string? university, int? budgetMin, int? budgetMax, TenantGender? gender, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var query = _db.HousingRequests
                .Where(r => r.IsActive && !r.IsClosed)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(city)) query = query.Where(r => r.City == city.Trim());
            if (!string.IsNullOrWhiteSpace(university)) query = query.Where(r => r.University == university.Trim());
            if (budgetMin.HasValue) query = query.Where(r => r.BudgetMax >= budgetMin.Value);
            if (budgetMax.HasValue) query = query.Where(r => r.BudgetMin <= budgetMax.Value);
            if (gender.HasValue && gender.Value != TenantGender.Any) query = query.Where(r => r.PreferredGender == TenantGender.Any || r.PreferredGender == gender.Value);

            query = query.OrderByDescending(r => r.CreatedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new ViewModels.Student.HousingRequestCardViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    City = r.City,
                    University = r.University,
                    PropertyType = r.PropertyType,
                    BudgetMin = r.BudgetMin,
                    BudgetMax = r.BudgetMax,
                    Bedrooms = r.Bedrooms,
                    PreferredGender = r.PreferredGender,
                    StudentName = r.StudentProfile.User.FirstName + " " + r.StudentProfile.User.LastName,
                    StudentIsVerified = r.StudentProfile.VerificationStatus == VerificationStatus.Verified,
                    CreatedAt = r.CreatedAt,
                    OffersCount = r.Offers.Count
                }).ToListAsync();

            return new ViewModels.PaginatedResult<ViewModels.Student.HousingRequestCardViewModel> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<IReadOnlyList<HousingRequest>> GetByStudentProfileAsync(int studentProfileId)
        {
            return await _db.HousingRequests
                .Where(r => r.StudentProfileId == studentProfileId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<HousingRequestOffer>> GetOffersForRequestAsync(int requestId)
        {
            return await _db.HousingRequestOffers
                .Include(o => o.Offerer)
                .Include(o => o.OfferedProperty)
                .Where(o => o.HousingRequestId == requestId)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
