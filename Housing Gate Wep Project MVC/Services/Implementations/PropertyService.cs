using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using StudentHousing.Data;
using StudentHousing.Helpers;
using StudentHousing.Mappings;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;
using StudentHousing.Settings;
using StudentHousing.ViewModels;
using StudentHousing.ViewModels.Owner;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Implementations
{
    public partial class PropertyService : IPropertyService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifications;
        private readonly IWebHostEnvironment _env;
        private readonly PropertyReviewSettings _reviewSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResource> _L;
        private readonly IMemoryCache _cache;

        public PropertyService(IUnitOfWork uow, INotificationService notifications, IWebHostEnvironment env,
            IOptions<PropertyReviewSettings> reviewSettings, UserManager<ApplicationUser> userManager,
            IStringLocalizer<SharedResource> L, IMemoryCache cache)
        {
            _uow = uow;
            _notifications = notifications;
            _env = env;
            _reviewSettings = reviewSettings.Value;
            _userManager = userManager;
            _L = L;
            _cache = cache;
        }

        public async Task<int> CountPendingApplicationsAsync(string ownerId)
        {
            var all = await _uow.Applications.GetPendingByOwnerAsync(ownerId);
            return all.Count;
        }

        // ---------- Private helpers ----------

        private static void ApplyRoomsToProperty(Property property, List<RoomFormViewModel> rooms)
        {
            foreach (var room in rooms)
            {
                if (string.IsNullOrWhiteSpace(room.Name))
                {
                    continue;
                }

                property.Rooms.Add(new Room
                {
                    Name = room.Name.Trim(),
                    RoomType = room.RoomType,
                    RentPerMonth = room.RentPerMonth,
                    Description = room.Description,
                    BathroomType = room.BathroomType,
                    NumberOfBeds = room.NumberOfBeds,
                    AvailableBeds = room.AvailableBeds,
                    IsAvailable = room.IsAvailable
                });
            }
        }

        private async Task ApplyRoomsToPropertyAsync(Property property, List<RoomFormViewModel> rooms)
        {
            var submittedIds = rooms.Where(r => r.Id > 0).Select(r => r.Id).ToHashSet();

            // Remove rooms that were removed from the form and have no applications yet.
            foreach (var existing in property.Rooms.Where(r => !submittedIds.Contains(r.Id)).ToList())
            {
                var hasApplications = await _uow.Applications.AnyAsync(a => a.RoomId == existing.Id);
                if (!hasApplications)
                {
                    property.Rooms.Remove(existing);
                }
            }

            foreach (var room in rooms)
            {
                if (room.Id > 0)
                {
                    var existing = property.Rooms.FirstOrDefault(r => r.Id == room.Id);
                    if (existing != null)
                    {
                        existing.Name = room.Name.Trim();
                        existing.RoomType = room.RoomType;
                        existing.RentPerMonth = room.RentPerMonth;
                        existing.Description = room.Description;
                        existing.BathroomType = room.BathroomType;
                        existing.NumberOfBeds = room.NumberOfBeds;
                        existing.AvailableBeds = room.AvailableBeds;
                        existing.IsAvailable = room.IsAvailable;
                    }
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(room.Name))
                {
                    property.Rooms.Add(new Room
                    {
                        Name = room.Name.Trim(),
                        RoomType = room.RoomType,
                        RentPerMonth = room.RentPerMonth,
                        Description = room.Description,
                        BathroomType = room.BathroomType,
                        NumberOfBeds = room.NumberOfBeds,
                        AvailableBeds = room.AvailableBeds,
                        IsAvailable = room.IsAvailable
                    });
                }
            }
        }

        private async Task ApplyAmenitiesAsync(Property property, List<int> selectedIds)
        {
            var wanted = (selectedIds ?? new List<int>()).Where(id => id > 0).Distinct().ToHashSet();
            var existing = property.Amenities.Select(a => a.Id).ToHashSet();

            var idsToAdd = wanted.Where(id => !existing.Contains(id)).ToHashSet();
            if (idsToAdd.Count > 0)
            {
                var toAdd = await _uow.Amenities.ListAsync(a => idsToAdd.Contains(a.Id));
                foreach (var amenity in toAdd) property.Amenities.Add(amenity);
            }

            foreach (var removed in property.Amenities.Where(a => !wanted.Contains(a.Id)).ToList())
            {
                property.Amenities.Remove(removed);
            }
        }

        /// <summary>
        /// Significant edits that push an approved listing back into review.
        /// Controlled centrally via <see cref="PropertyReviewSettings"/>.
        /// </summary>
        private bool HasSignificantChanges(Property property, PropertyFormViewModel model)
        {
            if (_reviewSettings.RequireReviewOnPropertyTypeChange && property.PropertyType != model.PropertyType)
            {
                return true;
            }

            if (_reviewSettings.RequireReviewOnLocationChange &&
                (!string.Equals(property.State, model.State, StringComparison.OrdinalIgnoreCase)
                 || !string.Equals(property.City, model.City, StringComparison.OrdinalIgnoreCase)
                 || !string.Equals(property.District, model.District, StringComparison.OrdinalIgnoreCase)
                 || !string.Equals(property.Address, model.Address, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (_reviewSettings.RequireReviewOnBedroomCountChange && property.Bedrooms != model.Bedrooms)
            {
                return true;
            }

            if (_reviewSettings.RequireReviewOnBathroomCountChange && property.Bathrooms != model.Bathrooms)
            {
                return true;
            }

            if (_reviewSettings.RequireReviewOnFurnishingChange && property.IsFurnished != model.IsFurnished)
            {
                return true;
            }

            if (property.AllowedGender != model.AllowedGender)
            {
                return true;
            }

            if (_reviewSettings.RequireReviewOnAmenityChange)
            {
                var current = property.Amenities.Select(a => a.Id).OrderBy(id => id).ToList();
                var next = (model.SelectedAmenityIds ?? new List<int>()).Where(id => id > 0).Distinct().OrderBy(id => id).ToList();
                if (!current.SequenceEqual(next))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Validates uploaded files up front so a bad image cannot 500 the save.
        /// Returns a localized error, or null when all files are acceptable.
        /// </summary>
        private string? ValidateImages(List<IFormFile>? uploads)
        {
            if (uploads == null) return null;
            foreach (var file in uploads)
            {
                if (file == null || file.Length == 0) continue;
                var error = ImageFileHelper.Validate(file, _L);
                if (error != null) return error;
            }
            return null;
        }

        private async Task SaveImagesAsync(Property property, List<IFormFile>? uploads)
        {
            if (uploads == null || uploads.Count == 0)
            {
                return;
            }

            var becamePrimary = !property.Images.Any();
            foreach (var file in uploads.Take(6))
            {
                if (file == null || file.Length == 0)
                {
                    continue;
                }

                var path = await ImageFileHelper.SaveAsync(file, _env);
                property.Images.Add(new PropertyImage
                {
                    PropertyId = property.Id,
                    FilePath = path,
                    IsPrimary = becamePrimary && !property.Images.Any(i => i.IsPrimary)
                });
            }
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("unique", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("IX_PropertyApplications_RoomId_StudentProfileId", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
        }
    }
}
