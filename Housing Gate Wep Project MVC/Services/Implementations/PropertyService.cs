using Microsoft.EntityFrameworkCore;
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
    public class PropertyService : IPropertyService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifications;
        private readonly IWebHostEnvironment _env;
        private readonly PropertyReviewSettings _reviewSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResource> _L;

        public PropertyService(IUnitOfWork uow, INotificationService notifications, IWebHostEnvironment env,
            IOptions<PropertyReviewSettings> reviewSettings, UserManager<ApplicationUser> userManager,
            IStringLocalizer<SharedResource> L)
        {
            _uow = uow;
            _notifications = notifications;
            _env = env;
            _reviewSettings = reviewSettings.Value;
            _userManager = userManager;
            _L = L;
        }

        // ---------- Public search / reading ----------

        public async Task<IReadOnlyList<PropertyCardViewModel>> GetForHomeAsync(int take = 6)
        {
            var properties = await _uow.Properties.GetApprovedActiveAsync();
            return properties
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(take)
                .Select(PropertyMapper.ToCard)
                .ToList();
        }

        public async Task<IReadOnlyList<PropertyCardViewModel>> SearchAsync(PropertySearchViewModel search)
        {
            var query = (await _uow.Properties.GetApprovedActiveAsync()).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.Trim();
                query = query.Where(p =>
                    p.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.City.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search.City))
            {
                query = query.Where(p => p.City.Equals(search.City, StringComparison.OrdinalIgnoreCase));
            }

            if (search.PropertyType.HasValue)
            {
                query = query.Where(p => p.PropertyType == search.PropertyType.Value);
            }

            if (search.MaxRent.HasValue)
            {
                query = query.Where(p =>
                    p.Rooms.Any(r => r.IsAvailable && r.RentPerMonth <= search.MaxRent.Value));
            }

            query = search.SortBy switch
            {
                "price-asc" => query.OrderBy(p => p.MinRoomRent),
                "price-desc" => query.OrderByDescending(p => p.MinRoomRent),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
            };

            return query.Select(PropertyMapper.ToCard).ToList();
        }

        public async Task<Property?> GetByIdWithDetailsAsync(int id)
            => await _uow.Properties.GetByIdWithDetailsAsync(id);

        public async Task<IReadOnlyList<string>> GetCitiesAsync()
        {
            var properties = await _uow.Properties.GetApprovedActiveAsync();
            return properties
                .Select(p => p.City)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public async Task<int> CountApprovedActiveAsync()
        {
            var properties = await _uow.Properties.GetApprovedActiveAsync();
            return properties.Count;
        }

        public async Task<IReadOnlyList<Amenity>> GetActiveAmenitiesAsync()
            => await _uow.Amenities.ListAsync(a => a.IsActive, orderBy: q => q.OrderBy(a => a.Name));

        // ---------- Owner management ----------

        public async Task<IReadOnlyList<Property>> GetByOwnerAsync(string ownerId)
            => await _uow.Properties.GetByOwnerAsync(ownerId);

        public async Task<(bool Success, string Error)> CreateAsync(PropertyFormViewModel model, string ownerId)
        {
            var property = new Property
            {
                OwnerId = ownerId,
                Title = model.Title,
                Description = model.Description,
                PropertyType = model.PropertyType,
                Address = model.Address,
                City = model.City,
                State = model.State,
                District = model.District,
                ZipCode = model.ZipCode,
                University = model.University,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Deposit = model.Deposit,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                IsFurnished = model.IsFurnished,
                PetAllowed = model.PetAllowed,
                AvailableFrom = model.AvailableFrom,
                ApprovalStatus = ApprovalStatus.Pending
            };

            ApplyRoomsToProperty(property, model.Rooms);
            var imageError = await ValidateImagesAsync(model.UploadedImages);
            if (imageError != null)
            {
                return (false, imageError);
            }

            await _uow.Properties.AddAsync(property);
            await _uow.SaveChangesAsync();

            property.PublicId = $"APT-{property.Id:D5}";
            await ApplyAmenitiesAsync(property, model.SelectedAmenityIds);
            await SaveImagesAsync(property, model.UploadedImages);
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> UpdateAsync(PropertyFormViewModel model, string ownerId)
        {
            var property = await _uow.Properties.GetByIdWithDetailsAsync(model.Id ?? 0);
            if (property == null || property.OwnerId != ownerId)
            {
                return (false, _L["Err.PropertyNotFound"]);
            }

            var significantChange = HasSignificantChanges(property, model);

            property.Title = model.Title;
            property.Description = model.Description;
            property.PropertyType = model.PropertyType;
            property.Address = model.Address;
            property.City = model.City;
            property.State = model.State;
            property.District = model.District;
            property.ZipCode = model.ZipCode;
            property.University = model.University;
            property.Latitude = model.Latitude;
            property.Longitude = model.Longitude;
            property.Deposit = model.Deposit;
            property.Bedrooms = model.Bedrooms;
            property.Bathrooms = model.Bathrooms;
            property.IsFurnished = model.IsFurnished;
            property.PetAllowed = model.PetAllowed;
            property.AvailableFrom = model.AvailableFrom;
            property.UpdatedAt = DateTime.UtcNow;

            if (property.PublicId == null)
            {
                property.PublicId = $"APT-{property.Id:D5}";
            }

            if (significantChange && property.ApprovalStatus == ApprovalStatus.Approved)
            {
                property.ApprovalStatus = ApprovalStatus.Pending;
                property.IsModifiedSinceApproval = true;

                // Let every admin know this listing needs a fresh review.
                var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
                foreach (var admin in admins)
                {
                    await _notifications.CreateAsync(
                        admin.Id,
                        _L["Notif.ListingEdited"],
                        _L["Notif.ListingEditedBody", property.Title],
                        "/Admin/Properties");
                }
            }

            await ApplyRoomsToPropertyAsync(property, model.Rooms);
            var imageError = await ValidateImagesAsync(model.UploadedImages);
            if (imageError != null)
            {
                return (false, imageError);
            }

            await ApplyAmenitiesAsync(property, model.SelectedAmenityIds);
            await SaveImagesAsync(property, model.UploadedImages);
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> ToggleActiveAsync(int propertyId, string ownerId)
        {
            var property = await _uow.Properties.GetByIdAsync(propertyId);
            if (property == null || property.OwnerId != ownerId)
            {
                return (false, _L["Err.PropertyNotFound"]);
            }

            property.IsActive = !property.IsActive;
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> DeleteAsync(int propertyId, string ownerId)
        {
            var property = await _uow.Properties.GetByIdWithDetailsAsync(propertyId);
            if (property == null || property.OwnerId != ownerId)
            {
                return (false, _L["Err.PropertyNotFound"]);
            }

            foreach (var image in property.Images)
            {
                ImageFileHelper.Delete(image.FilePath, _env);
            }

            _uow.Properties.Remove(property);
            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (Exception)
            {
                return (false, _L["Err.PropertyCannotBeDeleted"]);
            }

            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> DeleteImageAsync(int propertyId, int imageId, string ownerId)
        {
            var property = await _uow.Properties.GetByIdWithDetailsAsync(propertyId);
            if (property == null || property.OwnerId != ownerId)
            {
                return (false, _L["Err.PropertyNotFound"]);
            }

            var image = property.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return (false, _L["Err.ImageNotFound"]);
            }

            ImageFileHelper.Delete(image.FilePath, _env);
            property.Images.Remove(image);
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        // ---------- Applications ----------

        public async Task<IReadOnlyList<PropertyApplication>> GetApplicationsForOwnerAsync(string ownerId)
            => await _uow.Applications.GetByOwnerAsync(ownerId);

        public async Task<(bool Success, string Error)> ApplyAsync(int roomId, int studentProfileId, string? message)
        {
            var room = await _uow.Rooms.FirstOrDefaultAsync(r => r.Id == roomId, includeProperties: "Property.Owner");
            if (room == null)
            {
                return (false, _L["Err.RoomNotFound"]);
            }

            if (room.Property.ApprovalStatus != ApprovalStatus.Approved || !room.Property.IsActive)
            {
                return (false, _L["Err.ListingNotAccepting"]);
            }

            if (!room.IsAvailable)
            {
                return (false, _L["Err.RoomUnavailable"]);
            }

            var alreadyApplied = await _uow.Applications.AnyAsync(a =>
                a.RoomId == roomId && a.StudentProfileId == studentProfileId && a.Status != ApplicationStatus.Cancelled);
            if (alreadyApplied)
            {
                return (false, _L["Err.AlreadyApplied"]);
            }

            var application = new PropertyApplication
            {
                RoomId = roomId,
                StudentProfileId = studentProfileId,
                Message = message,
                Status = ApplicationStatus.Pending
            };
            await _uow.Applications.AddAsync(application);
            await _uow.SaveChangesAsync();

            await _notifications.CreateAsync(
                room.Property.OwnerId,
                _L["Notif.NewApplicationReceived"],
                _L["Notif.NewApplicationReceivedBody", room.Name, room.Property.Title],
                "/Owner/Applications");

            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> RespondToApplicationAsync(int applicationId, string ownerId, bool approve)
        {
            var application = await _uow.Applications.GetByIdWithDetailsAsync(applicationId);
            if (application == null || application.Room.Property.OwnerId != ownerId)
            {
                return (false, _L["Err.ApplicationNotFound"]);
            }

            if (application.Status != ApplicationStatus.Pending)
            {
                return (false, _L["Err.ApplicationAnswered"]);
            }

            application.Status = approve ? ApplicationStatus.Approved : ApplicationStatus.Rejected;
            application.RespondedAt = DateTime.UtcNow;

            if (approve)
            {
                var stay = new Stay
                {
                    RoomId = application.RoomId,
                    StudentProfileId = application.StudentProfileId,
                    ApplicationId = application.Id,
                    Status = StayStatus.Active,
                    StartDate = DateTime.UtcNow
                };
                await _uow.Stays.AddAsync(stay);

                var room = application.Room;
                room.IsAvailable = false;

                var studentProfile = application.StudentProfile;
                studentProfile.VerifiedStayCount++;
            }

            await _uow.SaveChangesAsync();

            await _notifications.CreateAsync(
                application.StudentProfile.UserId,
                approve ? _L["Notif.ApplicationAccepted"] : _L["Notif.ApplicationDeclined"],
                approve
                    ? _L["Notif.ApplicationAcceptedBody", application.Room.Name]
                    : _L["Notif.ApplicationDeclinedBody", application.Room.Name],
                "/Student/Applications");

            return (true, string.Empty);
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

            foreach (var id in wanted.Where(id => !existing.Contains(id)))
            {
                var amenity = await _uow.Amenities.FirstOrDefaultAsync(a => a.Id == id);
                if (amenity != null)
                {
                    property.Amenities.Add(amenity);
                }
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
        private async Task<string?> ValidateImagesAsync(List<IFormFile>? uploads)
        {
            if (uploads == null)
            {
                return null;
            }

            foreach (var file in uploads)
            {
                if (file == null || file.Length == 0)
                {
                    continue;
                }

                var error = ImageFileHelper.Validate(file, _L);
                if (error != null)
                {
                    return error;
                }
            }

            return await Task.FromResult<string?>(null);
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
    }
}
