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
    public class ComplaintsController : Controller
    {
        private readonly IComplaintService _complaintService;
        private readonly IUnitOfWork _uow;
        private readonly IStringLocalizer<SharedResource> _L;

        public ComplaintsController(IComplaintService complaintService, IUnitOfWork uow, IStringLocalizer<SharedResource> L)
        {
            _complaintService = complaintService;
            _uow = uow;
            _L = L;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var complaints = await _complaintService.GetByComplainantAsync(userId);
            return View(complaints);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ComplaintFormViewModel();
            await PopulateTargetsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComplaintFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTargetsAsync(model);
                return View(model);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _complaintService.FileAsync(model, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                await PopulateTargetsAsync(model);
                return View(model);
            }

            TempData["Success"] = _L["Msg.ComplaintFiled"].ToString();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var complaint = await _complaintService.GetByIdAsync(id);
            if (complaint == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            if (complaint.ComplainantId != userId && !User.IsInRole(AppRoles.Admin))
            {
                return Forbid();
            }

            return View(complaint);
        }

        private async Task PopulateTargetsAsync(ComplaintFormViewModel model)
        {
            var owners = await _uow.Users.GetOwnersWithProfilesAsync();
            var students = await _uow.Users.GetStudentsWithProfilesAsync();

            model.TargetUsers = owners
                .Concat(students)
                .DistinctBy(u => u.Id)
                .Select(u => new SelectListItemWrapper
                {
                    Value = u.Id,
                    Text = u.FullName
                })
                .OrderBy(i => i.Text)
                .ToList();

            model.TargetProperties = (await _uow.Properties.ListAsync(
                    filter: p => p.ApprovalStatus == ApprovalStatus.Approved,
                    orderBy: q => q.OrderBy(p => p.Title)))
                .Select(p => new SelectListItemWrapper
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Title} - {p.City}"
                })
                .ToList();
        }
    }
}
