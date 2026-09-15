using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using StudentHousing.DTOs;
using StudentHousing.Helpers;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IStringLocalizer<SharedResource> _L;

        public UsersController(IAdminService adminService, IStringLocalizer<SharedResource> L)
        {
            _adminService = adminService;
            _L = L;
        }

        public async Task<IActionResult> Index(string? q)
        {
            ViewBag.Query = q;
            var users = await _adminService.GetAllUsersWithRolesAsync(q);
            return View(users);
        }

        public async Task<IActionResult> Students(string? q)
        {
            ViewBag.Query = q;
            return View(await _adminService.GetStudentsAsync(q));
        }

        public async Task<IActionResult> Owners(string? q)
        {
            ViewBag.Query = q;
            return View(await _adminService.GetOwnersAsync(q));
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _adminService.GetUserDetailsAsync(id);
            if (user == null)
            {
                TempData["Error"] = _L["Err.UserNotFound"];
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            var result = await _adminService.DeleteUserAsync(userId);
            TempData[result.Success ? "Success" : "Error"] = result.Success ? _L["Admin.Users.Deleted"] : result.Error;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyStudent(string userId, bool approve, string? reason = null)
        {
            var result = await _adminService.VerifyStudentAsync(userId, approve, reason);
            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? (approve ? _L["Admin.Users.StudentVerified"] : _L["Admin.Users.StudentRejected"])
                : result.Error;
            return RedirectToAction(nameof(Students));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestDocuments(string userId, string? reason = null)
        {
            var result = await _adminService.RequestStudentDocumentsAsync(userId, reason);
            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? _L["Admin.Users.DocsRequested"]
                : result.Error;
            return RedirectToAction(nameof(Students));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOwner(string userId, bool approve)
        {
            await _adminService.VerifyOwnerAsync(userId, approve);
            return RedirectToAction(nameof(Owners));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string userId)
        {
            var result = await _adminService.ToggleUserActiveAsync(userId);
            TempData[result.Success ? "Success" : "Error"] = result.Success ? _L["Admin.Users.Updated"] : result.Error;
            return RedirectToAction(nameof(Index));
        }
    }
}
