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
    public class PropertiesController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IStringLocalizer<SharedResource> _L;

        public PropertiesController(IAdminService adminService, IStringLocalizer<SharedResource> L)
        {
            _adminService = adminService;
            _L = L;
        }

        public async Task<IActionResult> Index(ApprovalStatus? status, string? q)
        {
            var properties = await _adminService.GetPropertiesAsync(status, q);
            ViewBag.Filter = status;
            ViewBag.Query = q;
            return View(properties);
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _adminService.GetPropertyDetailsAsync(id);
            if (property == null)
            {
                TempData["Error"] = _L["Err.PropertyNotFound"];
                return RedirectToAction(nameof(Index));
            }
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _adminService.DeletePropertyAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Success ? _L["Admin.Props.Deleted"] : result.Error;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? note)
        {
            var result = await _adminService.ModeratePropertyAsync(id, approve: true, note);
            TempData[result.Success ? "Success" : "Error"] = result.Success ? _L["Admin.Props.Approved"] : result.Error;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string note)
        {
            var result = await _adminService.ModeratePropertyAsync(id, approve: false, note);
            TempData[result.Success ? "Success" : "Error"] = result.Success ? _L["Admin.Props.Rejected"] : result.Error;
            return RedirectToAction(nameof(Index));
        }
    }
}
