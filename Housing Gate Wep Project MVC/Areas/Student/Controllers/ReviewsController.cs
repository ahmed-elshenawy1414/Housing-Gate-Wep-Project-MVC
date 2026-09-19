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
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IStudentService _studentService;
        private readonly IUnitOfWork _uow;
        private readonly IStringLocalizer<SharedResource> _L;

        public ReviewsController(IReviewService reviewService, IStudentService studentService, IUnitOfWork uow, IStringLocalizer<SharedResource> L)
        {
            _reviewService = reviewService;
            _studentService = studentService;
            _uow = uow;
            _L = L;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var reviews = await _reviewService.GetByReviewerAsync(userId);
            var aboutMe = await _uow.Users.GetReviewsAboutUserAsync(userId);
            var writtenByMe = await _uow.Users.GetReviewsByUserAsync(userId);

            ViewBag.AboutMe = aboutMe;
            ViewBag.WrittenByMe = writtenByMe;
            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int stayId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var stay = await _uow.Stays.GetByIdWithDetailsAsync(stayId);
            if (stay == null || stay.StudentProfile.UserId != userId)
            {
                return NotFound();
            }

            var model = new PropertyReviewFormViewModel
            {
                StayId = stayId,
                PropertyId = stay.Room.PropertyId,
                PropertyTitle = stay.Room.Property.Title,
                RoomName = stay.Room.Name
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyReviewFormViewModel model)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            // Repopulate display helpers for redisplay
            var stayForView = await _uow.Stays.GetByIdWithDetailsAsync(model.StayId);
            if (stayForView != null)
            {
                model.PropertyTitle = stayForView.Room.Property.Title;
                model.RoomName = stayForView.Room.Name;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _reviewService.AddPropertyReviewAsync(model, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return View(model);
            }

            TempData["Success"] = _L["Msg.ReviewSubmitted"].ToString();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ReviewRoommate(int stayId, string userId)
        {
            var reviewerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var stay = await _uow.Stays.GetByIdWithDetailsAsync(stayId);
            if (stay == null || stay.StudentProfile.UserId != reviewerId)
            {
                return NotFound();
            }

            var roommates = await _reviewService.GetRoommatesAsync(stayId, reviewerId);
            var target = roommates.FirstOrDefault(r => r.Id == userId);
            if (target == null)
            {
                return NotFound(_L["Msg.NotSharedStay"]);
            }

            ViewBag.StayId = stayId;
            ViewBag.ReviewedUserId = userId;
            ViewBag.ReviewedUserName = target.FullName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewRoommate(int stayId, string userId, int rating, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = _L["Msg.CommentRequired"].ToString();
                return RedirectToAction(nameof(ReviewRoommate), new { stayId, userId });
            }

            var reviewerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _reviewService.AddUserReviewAsync(stayId, userId, reviewerId, rating, comment);

            if (!result.Success)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = _L["Msg.RoommateReviewSubmitted"].ToString();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
