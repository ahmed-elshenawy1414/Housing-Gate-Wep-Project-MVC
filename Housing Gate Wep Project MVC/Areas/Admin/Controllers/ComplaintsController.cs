using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class ComplaintsController : Controller
    {
        private readonly IComplaintService _complaintService;
        private readonly IStringLocalizer<SharedResource> _L;

        public ComplaintsController(IComplaintService complaintService, IStringLocalizer<SharedResource> L)
        {
            _complaintService = complaintService;
            _L = L;
        }

        public async Task<IActionResult> Index(string? q)
        {
            ViewBag.Query = q;
            return View(await _complaintService.GetAllAsync(q));
        }

        public async Task<IActionResult> Details(int id)
        {
            var complaint = await _complaintService.GetByIdAsync(id);
            if (complaint == null)
            {
                return NotFound();
            }

            return View(complaint);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int id, ComplaintStatus status, string? response)
        {
            var result = await _complaintService.RespondAsync(id, status, response);
            TempData[result.Success ? "Success" : "Error"] = result.Success ? _L["Admin.Cmpl.Updated"] : result.Error;
            return RedirectToAction(nameof(Index));
        }
    }
}
