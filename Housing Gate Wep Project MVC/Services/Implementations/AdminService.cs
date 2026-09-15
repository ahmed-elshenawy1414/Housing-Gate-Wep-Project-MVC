using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using StudentHousing.DTOs;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Admin;

namespace StudentHousing.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifications;
        private readonly IAuditLogService _auditLog;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResource> _L;
        private readonly IWebHostEnvironment _env;

        public AdminService(IUnitOfWork uow, INotificationService notifications, IAuditLogService auditLog,
            UserManager<ApplicationUser> userManager, IStringLocalizer<SharedResource> L,
            IWebHostEnvironment env)
        {
            _uow = uow;
            _notifications = notifications;
            _auditLog = auditLog;
            _userManager = userManager;
            _L = L;
            _env = env;
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var users = await _uow.Users.GetAllUsersWithRolesAsync();
            var students = await _uow.Users.GetStudentsWithProfilesAsync();
            var owners = await _uow.Users.GetOwnersWithProfilesAsync();
            var properties = await _uow.Properties.GetApprovedActiveAsync();

            return new AdminDashboardViewModel
            {
                TotalUsers = users.Count,
                TotalStudents = students.Count,
                TotalOwners = owners.Count,
                PendingOwnerVerifications = owners.Count(o => o.OwnerProfile!.VerificationStatus == VerificationStatus.Pending),
                PendingStudentVerifications = students.Count(s => s.StudentProfile!.VerificationStatus == VerificationStatus.Pending),
                PendingProperties = await _uow.Properties.CountByStatusAsync(ApprovalStatus.Pending),
                ApprovedProperties = await _uow.Properties.CountByStatusAsync(ApprovalStatus.Approved),
                OpenComplaints = await _uow.Complaints.CountByStatusAsync(ComplaintStatus.Open)
                                  + await _uow.Complaints.CountByStatusAsync(ComplaintStatus.UnderReview),
                PendingReviews = (await _uow.Reviews.GetPendingAsync()).Count,
                TotalListings = properties.Count,
                RecentlyAddedProperties = (await _uow.Properties.ListAsync(
                        orderBy: q => q.OrderByDescending(p => p.CreatedAt), includeProperties: "Owner.User"))
                    .Take(5).ToList(),
                RecentComplaints = (await _uow.Complaints.GetAllWithDetailsAsync()).Take(5).ToList(),
                RecentlyJoinedUsers = (await _uow.Users.GetRecentUsersAsync(5)).ToList(),
                RecentPendingReviews = (await _uow.Reviews.GetPendingAsync()).Take(5).ToList()
            };
        }

        public async Task<IReadOnlyList<UserWithRolesDto>> GetAllUsersWithRolesAsync(string? search = null)
            => await _uow.Users.GetAllUsersWithRolesAsync(search);

        public async Task<IReadOnlyList<ApplicationUser>> GetStudentsAsync(string? search = null)
            => await _uow.Users.GetStudentsWithProfilesAsync(search);

        public async Task<IReadOnlyList<ApplicationUser>> GetOwnersAsync(string? search = null)
            => await _uow.Users.GetOwnersWithProfilesAsync(search);

        public async Task<(bool Success, string Error)> VerifyStudentAsync(string userId, bool approve, string? reason = null)
        {
            var student = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            if (student == null)
            {
                return (false, _L["Err.StudentProfileNotFound"]);
            }

            student.VerificationStatus = approve ? VerificationStatus.Verified : VerificationStatus.Rejected;
            student.VerificationRejectReason = approve ? null : reason;
            student.VerifiedAt = approve ? DateTime.UtcNow : null;
            await _uow.SaveChangesAsync();

            await _notifications.CreateAsync(
                userId,
                approve ? _L["Notif.StudentVerified"] : _L["Notif.StudentRejected"],
                approve
                    ? _L["Notif.StudentVerifiedBody"]
                    : string.IsNullOrWhiteSpace(reason)
                        ? _L["Notif.StudentRejectedBody"]
                        : _L["Notif.ReasonPrefix", reason],
                "/Student/Profile");

            await _auditLog.LogAsync(
                approve ? "Student.Verify" : "Student.Reject",
                "Student",
                userId,
                await DescribeUserAsync(userId),
                reason);

            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> RequestStudentDocumentsAsync(string userId, string? reason)
        {
            var student = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            if (student == null)
            {
                return (false, _L["Err.StudentProfileNotFound"]);
            }

            student.VerificationStatus = VerificationStatus.NeedsChanges;
            student.VerificationRejectReason = reason;
            await _uow.SaveChangesAsync();

            await _notifications.CreateAsync(
                userId,
                _L["Notif.DocsRequested"],
                string.IsNullOrWhiteSpace(reason)
                    ? _L["Notif.DocsRequestedBody"]
                    : _L["Notif.ReasonPrefix", reason],
                "/Student/Profile");

            await _auditLog.LogAsync(
                "Student.RequestDocuments",
                "Student",
                userId,
                await DescribeUserAsync(userId),
                reason);

            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> VerifyOwnerAsync(string userId, bool approve)
        {
            var owner = await _uow.OwnerProfiles.FirstOrDefaultAsync(o => o.UserId == userId, includeProperties: "User");
            if (owner == null)
            {
                return (false, _L["Err.OwnerProfileNotFound"]);
            }

            owner.VerificationStatus = approve ? VerificationStatus.Verified : VerificationStatus.Rejected;
            owner.VerifiedAt = approve ? DateTime.UtcNow : null;
            await _uow.SaveChangesAsync();

            await _notifications.CreateAsync(
                userId,
                approve ? _L["Notif.OwnerVerified"] : _L["Notif.OwnerRejected"],
                approve
                    ? _L["Notif.OwnerVerifiedBody"]
                    : _L["Notif.OwnerRejectedBody"],
                "/Owner/Dashboard");

            await _auditLog.LogAsync(
                approve ? "Owner.Verify" : "Owner.Reject",
                "Owner",
                userId,
                await DescribeUserAsync(userId));

            return (true, string.Empty);
        }

        public async Task<IReadOnlyList<Property>> GetPropertiesAsync(ApprovalStatus? status = null, string? search = null)
        {
            return await _uow.Properties.SearchAsync(status, search);
        }

        public async Task<(bool Success, string Error)> ModeratePropertyAsync(int propertyId, bool approve, string? note)
        {
            var property = await _uow.Properties.GetByIdAsync(propertyId);
            if (property == null)
            {
                return (false, _L["Err.PropertyNotFound"]);
            }

            property.ApprovalStatus = approve ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
            property.AdminNote = note;
            property.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync();

            await _notifications.CreateAsync(
                property.OwnerId,
                approve ? _L["Notif.ListingApproved"] : _L["Notif.ListingRejected"],
                approve
                    ? _L["Notif.ListingApprovedBody", property.Title]
                    : _L["Notif.ListingRejectedBody", property.Title, note ?? _L["Notif.Unspecified"]],
                "/Owner/Properties");

            await _auditLog.LogAsync(
                approve ? "Property.Approve" : "Property.Reject",
                "Property",
                propertyId.ToString(),
                property.Title,
                note);

            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> ToggleUserActiveAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, _L["Err.UserNotFound"]);
            }

            if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                return (false, _L["Err.CannotDeactivateAdmin"]);
            }

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await _auditLog.LogAsync(
                user.IsActive ? "User.Activate" : "User.Suspend",
                "User",
                userId,
                user.Email);

            return (true, string.Empty);
        }

        public async Task<AdminUserDetailsViewModel?> GetUserDetailsAsync(string userId)
        {
            var user = await _uow.Users.GetUserWithProfileAsync(userId);
            if (user == null)
            {
                return null;
            }

            var vm = new AdminUserDetailsViewModel
            {
                User = user,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                OwnerProfile = user.OwnerProfile,
                StudentProfile = user.StudentProfile
            };

            if (user.OwnerProfile != null)
            {
                var properties = await _uow.Properties.GetByOwnerAsync(userId);
                vm.PropertyCount = properties.Count;
                foreach (var property in properties)
                {
                    if (await PropertyHasHistoryAsync(property.Id))
                    {
                        vm.BlockReasons.Add(_L["Err.UserPropertyHasHistory", property.Title]);
                    }
                }
            }

            if (user.StudentProfile != null)
            {
                var profileId = user.StudentProfile.Id;
                vm.ApplicationCount = await _uow.Applications.CountAsync(a => a.StudentProfileId == profileId);
                vm.StayCount = await _uow.Stays.CountAsync(s => s.StudentProfileId == profileId);
            }

            vm.ComplaintsFiled = await _uow.Complaints.CountAsync(c => c.ComplainantId == userId);
            vm.ComplaintsReceived = await _uow.Complaints.CountAsync(c => c.TargetUserId == userId);
            vm.UserReviewsGiven = (await _uow.Users.GetReviewsByUserAsync(userId)).Count;
            vm.UserReviewsReceived = (await _uow.Users.GetReviewsAboutUserAsync(userId)).Count;

            vm.CanDelete = await CanDeleteUserAsync(user, vm);
            return vm;
        }

        public async Task<(bool Success, string Error)> DeleteUserAsync(string userId)
        {
            var user = await _uow.Users.GetUserWithProfileAsync(userId);
            if (user == null)
            {
                return (false, _L["Err.UserNotFound"]);
            }

            var vm = new AdminUserDetailsViewModel { User = user, OwnerProfile = user.OwnerProfile, StudentProfile = user.StudentProfile };
            if (user.OwnerProfile != null)
            {
                var properties = await _uow.Properties.GetByOwnerAsync(userId);
                vm.PropertyCount = properties.Count;
                foreach (var property in properties)
                {
                    if (await PropertyHasHistoryAsync(property.Id))
                    {
                        vm.BlockReasons.Add(_L["Err.UserPropertyHasHistory", property.Title]);
                    }
                }
            }
            if (user.StudentProfile != null)
            {
                var profileId = user.StudentProfile.Id;
                vm.ApplicationCount = await _uow.Applications.CountAsync(a => a.StudentProfileId == profileId);
                vm.StayCount = await _uow.Stays.CountAsync(s => s.StudentProfileId == profileId);
            }
            vm.ComplaintsFiled = await _uow.Complaints.CountAsync(c => c.ComplainantId == userId);
            vm.ComplaintsReceived = await _uow.Complaints.CountAsync(c => c.TargetUserId == userId);
            vm.UserReviewsGiven = (await _uow.Users.GetReviewsByUserAsync(userId)).Count;
            vm.UserReviewsReceived = (await _uow.Users.GetReviewsAboutUserAsync(userId)).Count;

            if (!await CanDeleteUserAsync(user, vm))
            {
                return (false, string.Join(" ", vm.BlockReasons));
            }

            // Remove everything that has no history of its own; records with history block deletion above.
            var complaints = await _uow.Complaints.ListAsync(c => c.ComplainantId == userId || c.TargetUserId == userId);
            foreach (var complaint in complaints)
            {
                _uow.Complaints.Remove(complaint);
            }

            if (user.OwnerProfile != null)
            {
                var properties = await _uow.Properties.GetByOwnerAsync(userId);
                foreach (var property in properties)
                {
                    foreach (var image in property.Images)
                    {
                        ImageFileHelper.Delete(image.FilePath, _env);
                    }
                    _uow.Properties.Remove(property);
                }
                _uow.OwnerProfiles.Remove(user.OwnerProfile);
            }

            if (user.StudentProfile != null)
            {
                _uow.StudentProfiles.Remove(user.StudentProfile);
            }

            await _uow.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await _auditLog.LogAsync("User.Delete", "User", userId, user.Email);
            return (true, string.Empty);
        }

        public async Task<AdminPropertyDetailsViewModel?> GetPropertyDetailsAsync(int propertyId)
        {
            var property = await _uow.Properties.GetAdminDetailsAsync(propertyId);
            if (property == null)
            {
                return null;
            }

            var vm = new AdminPropertyDetailsViewModel
            {
                Property = property,
                ApplicationCount = await _uow.Applications.CountAsync(a => a.Room.PropertyId == propertyId),
                StayCount = await _uow.Stays.CountAsync(s => s.Room.PropertyId == propertyId),
                ComplaintCount = await _uow.Complaints.CountAsync(c => c.TargetPropertyId == propertyId)
            };

            vm.CanDelete = await CanDeletePropertyAsync(propertyId, vm);
            return vm;
        }

        public async Task<(bool Success, string Error)> DeletePropertyAsync(int propertyId)
        {
            var property = await _uow.Properties.GetByIdWithDetailsAsync(propertyId);
            if (property == null)
            {
                return (false, _L["Err.PropertyNotFound"]);
            }

            var vm = new AdminPropertyDetailsViewModel
            {
                ApplicationCount = await _uow.Applications.CountAsync(a => a.Room.PropertyId == propertyId),
                StayCount = await _uow.Stays.CountAsync(s => s.Room.PropertyId == propertyId),
                ComplaintCount = await _uow.Complaints.CountAsync(c => c.TargetPropertyId == propertyId)
            };

            if (!await CanDeletePropertyAsync(propertyId, vm))
            {
                return (false, string.Join(" ", vm.BlockReasons));
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

            await _notifications.CreateAsync(
                property.OwnerId,
                _L["Notif.ListingDeletedByAdmin"],
                _L["Notif.ListingDeletedByAdminBody", property.Title],
                "/Owner/Properties");

            await _auditLog.LogAsync("Property.Delete", "Property", propertyId.ToString(), property.Title);
            return (true, string.Empty);
        }

        private async Task<bool> PropertyHasHistoryAsync(int propertyId)
        {
            var hasApplications = await _uow.Applications.AnyAsync(a => a.Room.PropertyId == propertyId);
            var hasStays = await _uow.Stays.AnyAsync(s => s.Room.PropertyId == propertyId);
            var hasComplaints = await _uow.Complaints.AnyAsync(c => c.TargetPropertyId == propertyId);
            return hasApplications || hasStays || hasComplaints;
        }

        private async Task<bool> CanDeletePropertyAsync(int propertyId, AdminPropertyDetailsViewModel vm)
        {
            if (vm.ApplicationCount > 0)
            {
                vm.BlockReasons.Add(_L["Err.PropertyHasApplications"]);
            }
            if (vm.StayCount > 0)
            {
                vm.BlockReasons.Add(_L["Err.PropertyHasStays"]);
            }
            if (vm.ComplaintCount > 0)
            {
                vm.BlockReasons.Add(_L["Err.PropertyHasComplaints"]);
            }
            return vm.BlockReasons.Count == 0;
        }

        private async Task<bool> CanDeleteUserAsync(ApplicationUser user, AdminUserDetailsViewModel vm)
        {
            if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                vm.BlockReasons.Add(_L["Err.CannotDeleteAdmin"]);
            }
            if (vm.ApplicationCount > 0)
            {
                vm.BlockReasons.Add(_L["Err.UserHasApplications"]);
            }
            if (vm.StayCount > 0)
            {
                vm.BlockReasons.Add(_L["Err.UserHasStays"]);
            }
            if (vm.ComplaintsFiled + vm.ComplaintsReceived > 0)
            {
                vm.BlockReasons.Add(_L["Err.UserHasComplaints"]);
            }
            if (vm.UserReviewsGiven + vm.UserReviewsReceived > 0)
            {
                vm.BlockReasons.Add(_L["Err.UserHasReviews"]);
            }
            return vm.BlockReasons.Count == 0;
        }

        private async Task<string?> DescribeUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.Email;
        }
    }
}
