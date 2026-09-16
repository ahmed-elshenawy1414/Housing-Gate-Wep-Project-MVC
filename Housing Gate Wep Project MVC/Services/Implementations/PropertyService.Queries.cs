using Microsoft.Extensions.Caching.Memory;
using StudentHousing.Models;
using StudentHousing.ViewModels;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Implementations
{
    public partial class PropertyService
    {
        // ---------- Public search / reading (read-only, AsNoTracking, projection, cached) ----------

        public async Task<IReadOnlyList<PropertyCardViewModel>> GetForHomeAsync(int take = 6)
        {
            take = Math.Clamp(take, 1, 20);
            var cacheKey = $"home_cards_{take}";
            if (_cache.TryGetValue<IReadOnlyList<PropertyCardViewModel>>(cacheKey, out var cached)) return cached!;
            var result = await _uow.Properties.GetHomeCardsAsync(take);
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
            return result;
        }

        public async Task<IReadOnlyList<PropertyCardViewModel>> SearchAsync(PropertySearchViewModel search)
        {
            var page = Math.Max(1, search.Page);
            var pageSize = Math.Clamp(search.PageSize, 1, 50);
            var paged = await _uow.Properties.SearchCardsAsync(search, page, pageSize);
            search.TotalCount = paged.TotalCount;
            search.TotalPages = paged.TotalPages;
            search.Page = paged.Page;
            search.PageSize = paged.PageSize;
            return paged.Items;
        }

        public async Task<PaginatedResult<PropertyCardViewModel>> SearchPagedAsync(PropertySearchViewModel search)
        {
            var page = Math.Max(1, search.Page);
            var pageSize = Math.Clamp(search.PageSize, 1, 50);
            return await _uow.Properties.SearchCardsAsync(search, page, pageSize);
        }

        public async Task<IReadOnlyList<string>> GetCitiesAsync()
        {
            const string key = "distinct_cities";
            if (_cache.TryGetValue<IReadOnlyList<string>>(key, out var cached)) return cached!;
            var result = await _uow.Properties.GetDistinctCitiesAsync();
            _cache.Set(key, result, TimeSpan.FromMinutes(5));
            return result;
        }

        public async Task<int> CountApprovedActiveAsync()
        {
            return await _uow.Properties.CountApprovedActiveAsync();
        }

        public async Task<IReadOnlyList<Amenity>> GetActiveAmenitiesAsync()
        {
            const string key = "active_amenities";
            if (_cache.TryGetValue<IReadOnlyList<Amenity>>(key, out var cached)) return cached!;
            var result = await _uow.Amenities.ListAsync(a => a.IsActive, orderBy: q => q.OrderBy(a => a.Name));
            _cache.Set(key, result, TimeSpan.FromMinutes(5));
            return result;
        }

        public async Task<IReadOnlyList<Property>> GetByOwnerAsync(string ownerId)
            => await _uow.Properties.GetByOwnerAsync(ownerId);

        public async Task<Property?> GetByIdWithDetailsAsync(int id)
            => await _uow.Properties.GetByIdWithDetailsAsync(id);
    }
}
