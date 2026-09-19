using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentHousing.Helpers;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = AppRoles.Owner)]
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviews;
        private readonly IUnitOfWork _uow;

        public ReviewsController(IReviewService reviews, IUnitOfWork uow)
        {
            _reviews = reviews;
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var all = await _uow.Stays.GetByOwnerAsync(ownerId);
            var completed = all.Where(s => s.Status == Models.StayStatus.Completed).ToList();
            // Filter out stays already reviewed by this owner
            var filtered = new List<Models.Stay>();
            foreach (var s in completed)
            {
                if (!await _uow.Reviews.UserReviewExistsAsync(s.Id, ownerId, s.StudentProfile.UserId))
                    filtered.Add(s);
                if (filtered.Count >= 50) break;
            }
            return View(filtered);
        }

        [HttpGet]
        public async Task<IActionResult> ReviewStudent(int stayId)
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var stay = await _uow.Stays.GetByIdWithDetailsAsync(stayId);
            if (stay == null || stay.Room.Property.OwnerId != ownerId || stay.Status != Models.StayStatus.Completed)
                return NotFound();
            ViewBag.StayId = stayId;
            ViewBag.StudentName = stay.StudentProfile.User.FullName;
            ViewBag.StudentUserId = stay.StudentProfile.UserId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewStudent(int stayId, string reviewedUserId, int rating, string comment)
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _reviews.AddOwnerToStudentReviewAsync(stayId, reviewedUserId, ownerId, rating, comment);
            TempData[result.Success ? "Success" : "Error"] = result.Success ? "Review submitted for moderation." : result.Error;
            return RedirectToAction(nameof(Index));
        }
    }
}
