using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = AppRoles.Student)]
    public class StaysController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IReviewService _reviewService;
        private readonly IUnitOfWork _uow;

        public StaysController(IStudentService studentService, IReviewService reviewService, IUnitOfWork uow)
        {
            _studentService = studentService;
            _reviewService = reviewService;
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _studentService.GetMyProfileAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            var stays = await _uow.Stays.GetByStudentProfileAsync(profile.Id);

            var staysToReview = await _studentService.GetCompletedStaysWithoutReviewAsync(profile.Id);
            var stayIdsToReview = staysToReview.Select(s => s.Id).ToHashSet();

            var roommatesByStay = new Dictionary<int, IReadOnlyList<ApplicationUser>>();
            foreach (var stay in stays.Where(s => s.Status == StayStatus.Completed))
            {
                roommatesByStay[stay.Id] = await _reviewService.GetRoommatesAsync(stay.Id, userId);
            }

            ViewBag.StayIdsToReview = stayIdsToReview;
            ViewBag.RoommatesByStay = roommatesByStay;
            return View(stays);
        }
    }
}
