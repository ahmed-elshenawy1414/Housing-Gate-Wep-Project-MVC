using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Implementations
{
    public class ComplaintService : IComplaintService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifications;
        private readonly IAuditLogService _auditLog;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResource> _L;

        public ComplaintService(IUnitOfWork uow, INotificationService notifications, IAuditLogService auditLog,
            UserManager<ApplicationUser> userManager, IStringLocalizer<SharedResource> L)
        {
            _uow = uow;
            _notifications = notifications;
            _auditLog = auditLog;
            _userManager = userManager;
            _L = L;
        }

        public async Task<(bool Success, string Error)> FileAsync(ComplaintFormViewModel model, string complainantId)
        {
            var complaint = new Complaint
            {
                ComplainantId = complainantId,
                Subject = model.Subject,
                ComplaintType = model.ComplaintType,
                Description = model.Description,
                TargetUserId = string.IsNullOrWhiteSpace(model.TargetUserId) ? null : model.TargetUserId,
                TargetPropertyId = model.TargetPropertyId,
                Status = ComplaintStatus.Open
            };

            await _uow.Complaints.AddAsync(complaint);
            await _uow.SaveChangesAsync();

            // Let every admin know a complaint arrived.
            var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
            foreach (var admin in admins)
            {
                await _notifications.CreateAsync(
                    admin.Id,
                    _L["Notif.NewComplaint"],
                    _L["Notif.NewComplaintBody", complaint.Subject],
                    "/Admin/Complaints");
            }

            return (true, string.Empty);
        }

        public async Task<IReadOnlyList<Complaint>> GetByComplainantAsync(string userId)
            => await _uow.Complaints.GetByComplainantAsync(userId);

        public async Task<IReadOnlyList<Complaint>> GetAllAsync(string? search = null)
            => await _uow.Complaints.GetAllWithDetailsAsync(search);

        public async Task<Complaint?> GetByIdAsync(int id)
            => await _uow.Complaints.GetByIdWithDetailsAsync(id);

        public async Task<(bool Success, string Error)> RespondAsync(int complaintId, ComplaintStatus status, string? response)
        {
            var complaint = await _uow.Complaints.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                return (false, _L["Err.ComplaintNotFound"]);
            }

            complaint.Status = status;
            complaint.AdminResponse = response;
            complaint.ResolvedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync();

            await _notifications.CreateAsync(
                complaint.ComplainantId,
                _L["Notif.ComplaintUpdate"],
                _L["Notif.ComplaintUpdateBody", complaint.Subject, EnumDisplay.Get(status, _L)],
                "/Student/Complaints");

            await _auditLog.LogAsync(
                "Complaint.Respond",
                "Complaint",
                complaintId.ToString(),
                $"{complaint.Subject} ({status})",
                response);

            return (true, string.Empty);
        }
    }
}
