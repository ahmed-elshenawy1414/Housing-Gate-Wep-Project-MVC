using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = AppRoles.Student)]
    public class ApplicationsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifications;
        private readonly IStringLocalizer<SharedResource> _L;

        public ApplicationsController(IStudentService studentService, IUnitOfWork uow, INotificationService notifications, IStringLocalizer<SharedResource> L)
        {
            _studentService = studentService;
            _uow = uow;
            _notifications = notifications;
            _L = L;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _studentService.GetMyProfileAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            var applications = await _uow.Applications.GetByStudentProfileAsync(profile.Id);
            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _studentService.GetMyProfileAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            var application = await _uow.Applications.GetByIdWithDetailsAsync(id);
            if (application == null || application.StudentProfileId != profile.Id)
            {
                return NotFound();
            }

            if (application.Status != ApplicationStatus.Pending)
            {
                TempData["Error"] = _L["Msg.CancelPendingOnly"].ToString();
                return RedirectToAction(nameof(Index));
            }

            application.Status = ApplicationStatus.Cancelled;
            await _uow.SaveChangesAsync();

            await _notifications.CreateAsync(
                application.Room.Property.OwnerId,
                _L["Notif.ApplicationCancelled"],
                _L["Notif.ApplicationCancelledBody"],
                "/Owner/Applications");

            TempData["Success"] = _L["Msg.ApplicationCancelled"].ToString();
            return RedirectToAction(nameof(Index));
        }
    }
}
