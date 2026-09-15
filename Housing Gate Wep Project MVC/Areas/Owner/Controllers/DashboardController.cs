using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentHousing.Helpers;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = AppRoles.Owner)]
    public class DashboardController : Controller
    {
        private readonly IOwnerService _ownerService;

        public DashboardController(IOwnerService ownerService)
        {
            _ownerService = ownerService;
        }

        public async Task<IActionResult> Index()
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var model = await _ownerService.GetDashboardAsync(ownerId);
            return View(model);
        }
    }
}
