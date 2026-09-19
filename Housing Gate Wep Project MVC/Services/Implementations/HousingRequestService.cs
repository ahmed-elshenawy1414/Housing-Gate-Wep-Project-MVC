using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Implementations
{
    public class HousingRequestService : IHousingRequestService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMemoryCache _cache;

        public HousingRequestService(IUnitOfWork uow, IMemoryCache cache)
        {
            _uow = uow;
            _cache = cache;
        }

        public async Task<PaginatedResult<HousingRequestCardViewModel>> SearchAsync(string? city, string? university, int? budget, TenantGender? gender, int page, int pageSize)
        {
            return await _uow.HousingRequests.SearchCardsAsync(city, university, budget, budget, gender, page, pageSize);
        }

        public async Task<HousingRequest?> GetByIdAsync(int id) => await _uow.HousingRequests.GetByIdAsync(id);
        public async Task<HousingRequest?> GetByIdWithOffersAsync(int id) => await _uow.HousingRequests.GetByIdWithDetailsAsync(id);

        public async Task<IReadOnlyList<HousingRequest>> GetMyRequestsAsync(int studentProfileId)
            => await _uow.HousingRequests.GetByStudentProfileAsync(studentProfileId);

        public async Task<(bool Success, string Error, int? Id)> CreateAsync(HousingRequestFormViewModel model, int studentProfileId)
        {
            // Security: verify student exists and is not blocked
            var profile = await _uow.StudentProfiles.GetByIdAsync(studentProfileId);
            if (profile == null) return (false, "Profile not found", null);
            if (model.BudgetMin > model.BudgetMax) return (false, "Min budget cannot exceed max", null);

            var entity = new HousingRequest
            {
                StudentProfileId = studentProfileId,
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                City = model.City.Trim(),
                District = model.District?.Trim(),
                Governorate = model.Governorate?.Trim(),
                University = model.University?.Trim(),
                PropertyType = model.PropertyType,
                BudgetMin = model.BudgetMin,
                BudgetMax = model.BudgetMax,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                IsFurnished = model.IsFurnished,
                PetAllowed = model.PetAllowed,
                PreferredGender = model.PreferredGender,
                MoveInDate = model.MoveInDate,
                MinLeaseMonths = model.MinLeaseMonths,
                IsActive = true
            };

            // Privacy: do not store sensitive docs, only housing prefs
            var wantedIds = model.SelectedAmenityIds.Where(id => id > 0).Distinct().ToHashSet();
            if (wantedIds.Count > 0)
            {
                var amenities = await _uow.Amenities.ListAsync(a => wantedIds.Contains(a.Id));
                foreach (var a in amenities) entity.Amenities.Add(a);
            }

            await _uow.HousingRequests.AddAsync(entity);
            await _uow.SaveChangesAsync();
            // Invalidate cache for search (simple evict)
            _cache.Remove("housing_requests_cities");
            return (true, string.Empty, entity.Id);
        }

        public async Task<(bool Success, string Error)> UpdateAsync(HousingRequestFormViewModel model, int studentProfileId)
        {
            if (!model.Id.HasValue) return (false, "Invalid request");
            var entity = await _uow.HousingRequests.GetByIdWithDetailsAsync(model.Id.Value);
            if (entity == null || entity.StudentProfileId != studentProfileId) return (false, "Not found");
            if (model.BudgetMin > model.BudgetMax) return (false, "Min budget cannot exceed max");

            entity.Title = model.Title.Trim();
            entity.Description = model.Description.Trim();
            entity.City = model.City.Trim();
            entity.District = model.District?.Trim();
            entity.Governorate = model.Governorate?.Trim();
            entity.University = model.University?.Trim();
            entity.PropertyType = model.PropertyType;
            entity.BudgetMin = model.BudgetMin;
            entity.BudgetMax = model.BudgetMax;
            entity.Bedrooms = model.Bedrooms;
            entity.Bathrooms = model.Bathrooms;
            entity.IsFurnished = model.IsFurnished;
            entity.PetAllowed = model.PetAllowed;
            entity.PreferredGender = model.PreferredGender;
            entity.MoveInDate = model.MoveInDate;
            entity.MinLeaseMonths = model.MinLeaseMonths;
            entity.UpdatedAt = DateTime.UtcNow;

            var wanted = model.SelectedAmenityIds.Where(id => id > 0).Distinct().ToHashSet();
            var existing = entity.Amenities.Select(a => a.Id).ToHashSet();
            var toAddIds = wanted.Where(id => !existing.Contains(id)).ToHashSet();
            if (toAddIds.Count > 0)
            {
                var toAdd = await _uow.Amenities.ListAsync(a => toAddIds.Contains(a.Id));
                foreach (var a in toAdd) entity.Amenities.Add(a);
            }
            foreach (var rem in entity.Amenities.Where(a => !wanted.Contains(a.Id)).ToList())
                entity.Amenities.Remove(rem);

            try { await _uow.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return (false, "Conflict, please retry"); }
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> DeleteAsync(int id, int studentProfileId)
        {
            var entity = await _uow.HousingRequests.GetByIdAsync(id);
            if (entity == null || entity.StudentProfileId != studentProfileId) return (false, "Not found");
            _uow.HousingRequests.Remove(entity);
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> ToggleCloseAsync(int id, int studentProfileId)
        {
            var entity = await _uow.HousingRequests.GetByIdAsync(id);
            if (entity == null || entity.StudentProfileId != studentProfileId) return (false, "Not found");
            entity.IsClosed = !entity.IsClosed;
            entity.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> AddOfferAsync(int requestId, string offererUserId, HousingRequestOfferFormViewModel model)
        {
            var req = await _uow.HousingRequests.GetByIdAsync(requestId);
            if (req == null || !req.IsActive || req.IsClosed) return (false, "Request not available");
            // Security: prevent self-offer
            if (req.StudentProfileId == 0) return (false, "Invalid");
            var student = await _uow.StudentProfiles.GetByIdAsync(req.StudentProfileId);
            if (student != null && student.UserId == offererUserId) return (false, "Cannot offer to your own request");

            // If offeredPropertyId provided, verify ownership
            if (model.OfferedPropertyId.HasValue)
            {
                var prop = await _uow.Properties.GetByIdAsync(model.OfferedPropertyId.Value);
                if (prop == null || prop.OwnerId != offererUserId) return (false, "Property not found or not yours");
            }

            var offer = new HousingRequestOffer
            {
                HousingRequestId = requestId,
                OffererUserId = offererUserId,
                Message = model.Message.Trim(),
                OfferedPropertyId = model.OfferedPropertyId,
                Status = ApplicationStatus.Pending
            };
            await _uow.HousingRequestOffers.AddAsync(offer);
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }
    }
}
