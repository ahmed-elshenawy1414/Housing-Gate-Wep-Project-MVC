using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = AppRoles.Student)]
    public class DashboardController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IUnitOfWork _uow;
        private readonly IStringLocalizer<SharedResource> _L;

        public DashboardController(IStudentService studentService, IUnitOfWork uow, IStringLocalizer<SharedResource> L)
        {
            _studentService = studentService;
            _uow = uow;
            _L = L;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _studentService.GetMyProfileAsync(userId);
            if (profile == null)
            {
                return NotFound(_L["Err.StudentProfileNotFound"]);
            }

            var applications = await _uow.Applications.GetByStudentProfileAsync(profile.Id);
            var stays = await _uow.Stays.GetByStudentProfileAsync(profile.Id);
            var toReview = await _studentService.GetCompletedStaysWithoutReviewAsync(profile.Id);

            var model = new StudentDashboardViewModel
            {
                Profile = profile,
                ActiveApplications = applications.Count(a => a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Approved),
                ActiveStays = stays.Count(s => s.Status == StayStatus.Active),
                CompletedStays = stays.Count(s => s.Status == StayStatus.Completed),
                PendingReviews = toReview.Count,
                RecentApplications = applications.Take(5).ToList(),
                RecentStays = stays.Take(5).ToList()
            };

            return View(model);
        }
    }
}
