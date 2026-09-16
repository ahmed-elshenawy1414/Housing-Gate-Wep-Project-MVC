using Microsoft.EntityFrameworkCore;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.ViewModels.Owner;

namespace StudentHousing.Services.Implementations
{
    public partial class PropertyService
    {
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
            var imageError = ValidateImages(model.UploadedImages);
            if (imageError != null) return (false, imageError);

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
            if (property == null || property.OwnerId != ownerId) return (false, _L["Err.PropertyNotFound"]);

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
            if (property.PublicId == null) property.PublicId = $"APT-{property.Id:D5}";

            if (significantChange && property.ApprovalStatus == ApprovalStatus.Approved)
            {
                property.ApprovalStatus = ApprovalStatus.Pending;
                property.IsModifiedSinceApproval = true;
                var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
                if (admins.Count > 0)
                {
                    await _notifications.CreateManyAsync(admins.Select(a =>
                        (a.Id, _L["Notif.ListingEdited"].ToString(), _L["Notif.ListingEditedBody", property.Title].ToString(), (string?)"/Admin/Properties")));
                }
            }

            await ApplyRoomsToPropertyAsync(property, model.Rooms);
            var imageError = ValidateImages(model.UploadedImages);
            if (imageError != null) return (false, imageError);

            await ApplyAmenitiesAsync(property, model.SelectedAmenityIds);
            await SaveImagesAsync(property, model.UploadedImages);
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> ToggleActiveAsync(int propertyId, string ownerId)
        {
            var property = await _uow.Properties.GetByIdAsync(propertyId);
            if (property == null || property.OwnerId != ownerId) return (false, _L["Err.PropertyNotFound"]);
            property.IsActive = !property.IsActive;
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> DeleteAsync(int propertyId, string ownerId)
        {
            var property = await _uow.Properties.GetByIdWithDetailsAsync(propertyId);
            if (property == null || property.OwnerId != ownerId) return (false, _L["Err.PropertyNotFound"]);
            foreach (var image in property.Images) ImageFileHelper.Delete(image.FilePath, _env);
            _uow.Properties.Remove(property);
            try { await _uow.SaveChangesAsync(); }
            catch (DbUpdateException) { return (false, _L["Err.PropertyCannotBeDeleted"]); }
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> DeleteImageAsync(int propertyId, int imageId, string ownerId)
        {
            var property = await _uow.Properties.GetByIdWithDetailsAsync(propertyId);
            if (property == null || property.OwnerId != ownerId) return (false, _L["Err.PropertyNotFound"]);
            var image = property.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null) return (false, _L["Err.ImageNotFound"]);
            ImageFileHelper.Delete(image.FilePath, _env);
            property.Images.Remove(image);
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<IReadOnlyList<PropertyApplication>> GetApplicationsForOwnerAsync(string ownerId)
            => await _uow.Applications.GetByOwnerAsync(ownerId);

        public async Task<(bool Success, string Error)> ApplyAsync(int roomId, int studentProfileId, string? message)
        {
            var room = await _uow.Rooms.FirstOrDefaultAsync(r => r.Id == roomId, includeProperties: "Property.Owner");
            if (room == null) return (false, _L["Err.RoomNotFound"]);
            if (room.Property.ApprovalStatus != ApprovalStatus.Approved || !room.Property.IsActive) return (false, _L["Err.ListingNotAccepting"]);
            if (!room.IsAvailable) return (false, _L["Err.RoomUnavailable"]);
            var alreadyApplied = await _uow.Applications.AnyAsync(a => a.RoomId == roomId && a.StudentProfileId == studentProfileId && a.Status != ApplicationStatus.Cancelled);
            if (alreadyApplied) return (false, _L["Err.AlreadyApplied"]);
            var application = new PropertyApplication { RoomId = roomId, StudentProfileId = studentProfileId, Message = message, Status = ApplicationStatus.Pending };
            await _uow.Applications.AddAsync(application);
            try { await _uow.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex)) { return (false, _L["Err.AlreadyApplied"]); }
            catch (DbUpdateConcurrencyException) { return (false, _L["Err.AlreadyApplied"]); }
            await _notifications.CreateAsync(room.Property.OwnerId, _L["Notif.NewApplicationReceived"], _L["Notif.NewApplicationReceivedBody", room.Name, room.Property.Title], "/Owner/Applications");
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> RespondToApplicationAsync(int applicationId, string ownerId, bool approve)
        {
            var application = await _uow.Applications.GetByIdWithDetailsAsync(applicationId);
            if (application == null || application.Room.Property.OwnerId != ownerId) return (false, _L["Err.ApplicationNotFound"]);
            if (application.Status != ApplicationStatus.Pending) return (false, _L["Err.ApplicationAnswered"]);
            application.Status = approve ? ApplicationStatus.Approved : ApplicationStatus.Rejected;
            application.RespondedAt = DateTime.UtcNow;
            if (approve)
            {
                if (!application.Room.IsAvailable || application.Room.AvailableBeds <= 0) return (false, _L["Err.RoomUnavailable"]);
                var stay = new Stay { RoomId = application.RoomId, StudentProfileId = application.StudentProfileId, ApplicationId = application.Id, Status = StayStatus.Active, StartDate = DateTime.UtcNow };
                await _uow.Stays.AddAsync(stay);
                var room = application.Room;
                if (room.NumberOfBeds <= 1) room.IsAvailable = false;
                else { room.AvailableBeds = Math.Max(0, room.AvailableBeds - 1); if (room.AvailableBeds == 0) room.IsAvailable = false; }
                application.StudentProfile.VerifiedStayCount++;
            }
            try { await _uow.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return (false, _L["Err.ConcurrencyConflict"]); }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex)) { return (false, _L["Err.ConcurrencyConflict"]); }
            await _notifications.CreateAsync(application.StudentProfile.UserId, approve ? _L["Notif.ApplicationAccepted"] : _L["Notif.ApplicationDeclined"], approve ? _L["Notif.ApplicationAcceptedBody", application.Room.Name] : _L["Notif.ApplicationDeclinedBody", application.Room.Name], "/Student/Applications");
            return (true, string.Empty);
        }
    }
}
