using Microsoft.EntityFrameworkCore;
using StudentHousing.Models;
using StudentHousing.Data;
using StudentHousing.Repositories.Interfaces;

namespace StudentHousing.Repositories.Implementations
{
    public class PropertyRepository : Repository<Property>, IPropertyRepository
    {
        public PropertyRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<Property?> GetByIdWithDetailsAsync(int id)
        {
            return await _db.Properties
                .Include(p => p.Owner.User)
                .Include(p => p.Images)
                .Include(p => p.Rooms)
                .Include(p => p.Amenities)
                .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Approved)).ThenInclude(r => r.Reviewer)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Property?> GetAdminDetailsAsync(int id)
        {
            return await _db.Properties
                .Include(p => p.Owner.User)
                .Include(p => p.Images)
                .Include(p => p.Amenities)
                .Include(p => p.Rooms).ThenInclude(r => r.Beds)
                .Include(p => p.Reviews).ThenInclude(r => r.Reviewer)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IReadOnlyList<Property>> GetApprovedActiveAsync()
        {
            return await _db.Properties
                .Include(p => p.Owner.User)
                .Include(p => p.Images)
                .Include(p => p.Rooms)
                .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Approved))
                .Where(p => p.ApprovalStatus == ApprovalStatus.Approved && p.IsActive)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();
        }

        // Phase 4: efficient server-side card queries
        public async Task<StudentHousing.ViewModels.PaginatedResult<StudentHousing.ViewModels.PropertyCardViewModel>> SearchCardsAsync(StudentHousing.ViewModels.Student.PropertySearchViewModel search, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _db.Properties
                .Where(p => p.ApprovalStatus == ApprovalStatus.Approved && p.IsActive)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var kw = $"%{search.Keyword.Trim()}%";
                query = query.Where(p =>
                    EF.Functions.Like(p.Title, kw) ||
                    EF.Functions.Like(p.Description, kw) ||
                    EF.Functions.Like(p.City, kw));
            }

            if (!string.IsNullOrWhiteSpace(search.City))
            {
                var city = search.City.Trim();
                query = query.Where(p => p.City == city);
            }

            if (search.PropertyType.HasValue)
            {
                query = query.Where(p => p.PropertyType == search.PropertyType.Value);
            }

            if (search.MaxRent.HasValue)
            {
                var max = search.MaxRent.Value;
                query = query.Where(p => p.Rooms.Any(r => r.IsAvailable && r.RentPerMonth <= max));
            }

            query = search.SortBy switch
            {
                "price-asc" => query.OrderBy(p => p.Rooms.Where(r => r.IsAvailable).Min(r => (int?)r.RentPerMonth) ?? int.MaxValue),
                "price-desc" => query.OrderByDescending(p => p.Rooms.Where(r => r.IsAvailable).Min(r => (int?)r.RentPerMonth) ?? 0),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new StudentHousing.ViewModels.PropertyCardViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    City = p.City,
                    State = p.State,
                    PropertyType = p.PropertyType,
                    MinRent = p.Rooms.Where(r => r.IsAvailable).Min(r => (int?)r.RentPerMonth) ?? 0,
                    Bedrooms = p.Bedrooms,
                    IsFurnished = p.IsFurnished,
                    IsFeatured = p.IsFeatured,
                    AvailableFrom = p.AvailableFrom,
                    PrimaryImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.FilePath).FirstOrDefault()
                                      ?? p.Images.Select(i => i.FilePath).FirstOrDefault(),
                    OwnerName = p.Owner.User.FirstName + " " + p.Owner.User.LastName,
                    OwnerIsVerified = p.Owner.VerificationStatus == VerificationStatus.Verified,
                    AverageRating = p.Reviews.Where(r => r.Status == ReviewStatus.Approved).Average(r => (double?)r.Rating) ?? 0,
                    ReviewsCount = p.Reviews.Count(r => r.Status == ReviewStatus.Approved)
                })
                .ToListAsync();

            // Round average to 1 decimal in memory (SQL AVG already)
            foreach (var c in items) c.AverageRating = Math.Round(c.AverageRating, 1);

            return new StudentHousing.ViewModels.PaginatedResult<StudentHousing.ViewModels.PropertyCardViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<StudentHousing.ViewModels.PropertyCardViewModel>> GetHomeCardsAsync(int take)
        {
            take = Math.Clamp(take, 1, 20);
            return await _db.Properties
                .Where(p => p.ApprovalStatus == ApprovalStatus.Approved && p.IsActive)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(take)
                .Select(p => new StudentHousing.ViewModels.PropertyCardViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    City = p.City,
                    State = p.State,
                    PropertyType = p.PropertyType,
                    MinRent = p.Rooms.Where(r => r.IsAvailable).Min(r => (int?)r.RentPerMonth) ?? 0,
                    Bedrooms = p.Bedrooms,
                    IsFurnished = p.IsFurnished,
                    IsFeatured = p.IsFeatured,
                    AvailableFrom = p.AvailableFrom,
                    PrimaryImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.FilePath).FirstOrDefault()
                                      ?? p.Images.Select(i => i.FilePath).FirstOrDefault(),
                    OwnerName = p.Owner.User.FirstName + " " + p.Owner.User.LastName,
                    OwnerIsVerified = p.Owner.VerificationStatus == VerificationStatus.Verified,
                    AverageRating = p.Reviews.Where(r => r.Status == ReviewStatus.Approved).Average(r => (double?)r.Rating) ?? 0,
                    ReviewsCount = p.Reviews.Count(r => r.Status == ReviewStatus.Approved)
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetDistinctCitiesAsync()
        {
            return await _db.Properties
                .Where(p => p.ApprovalStatus == ApprovalStatus.Approved && p.IsActive)
                .Select(p => p.City)
                .Distinct()
                .OrderBy(c => c)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountApprovedActiveAsync()
        {
            return await _db.Properties.CountAsync(p => p.ApprovalStatus == ApprovalStatus.Approved && p.IsActive);
        }

        public async Task<IReadOnlyList<Property>> GetByOwnerAsync(string ownerId)
        {
            return await _db.Properties
                .Include(p => p.Images)
                .Include(p => p.Rooms)
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountByStatusAsync(ApprovalStatus status)
        {
            return await _db.Properties.CountAsync(p => p.ApprovalStatus == status);
        }

        public async Task<IReadOnlyList<Property>> SearchAsync(ApprovalStatus? status, string? search)
        {
            var query = _db.Properties
                .Include(p => p.Owner.User)
                .Include(p => p.Images)
                .Include(p => p.Rooms)
                .AsNoTracking()
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.ApprovalStatus == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(s)
                    || p.City.ToLower().Contains(s)
                    || p.Address.ToLower().Contains(s)
                    || (p.PublicId != null && p.PublicId.ToLower().Contains(s))
                    || p.Owner.User.FirstName.ToLower().Contains(s)
                    || p.Owner.User.LastName.ToLower().Contains(s)
                    || (p.Owner.User.Email != null && p.Owner.User.Email.ToLower().Contains(s)));
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }
    }
}
