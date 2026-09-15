using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IUnitOfWork _uow;

        public ReviewsController(IReviewService reviewService, IUnitOfWork uow)
        {
            _reviewService = reviewService;
            _uow = uow;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var pending = await _reviewService.GetPendingAsync();

            Expression<Func<PropertyReview, bool>> filter = r => r.Status != ReviewStatus.Pending;
            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim().ToLower();
                filter = r => r.Status != ReviewStatus.Pending
                    && (r.Title.ToLower().Contains(s)
                        || r.Comment.ToLower().Contains(s)
                        || r.Property.Title.ToLower().Contains(s)
                        || r.Reviewer.FirstName.ToLower().Contains(s)
                        || r.Reviewer.LastName.ToLower().Contains(s));
            }

            var all = await _uow.Reviews.ListAsync(
                filter: filter,
                orderBy: q => q.OrderByDescending(r => r.CreatedAt),
                includeProperties: "Reviewer,Property");

            ViewBag.Pending = pending;
            ViewBag.Query = q;
            return View(all);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            await _reviewService.ModerateAsync(id, approve: true);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            await _reviewService.ModerateAsync(id, approve: false);
            return RedirectToAction(nameof(Index));
        }
    }
}
