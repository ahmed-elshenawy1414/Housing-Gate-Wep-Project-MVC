using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = AppRoles.Owner)]
    public class ApplicationsController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IStringLocalizer<SharedResource> _L;

        public ApplicationsController(IPropertyService propertyService, IStringLocalizer<SharedResource> L)
        {
            _propertyService = propertyService;
            _L = L;
        }

        public async Task<IActionResult> Index()
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var applications = await _propertyService.GetApplicationsForOwnerAsync(ownerId);
            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int id, bool approve)
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _propertyService.RespondToApplicationAsync(id, ownerId, approve);

            TempData[result.Success ? "Success" : "Error"] =
                result.Success
                    ? (approve ? _L["Msg.ApplicationApproved"].ToString() : _L["Msg.ApplicationRejected"].ToString())
                    : result.Error;

            return RedirectToAction(nameof(Index));
        }
    }
}
