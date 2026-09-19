using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Controllers
{
    [Authorize]
    public class HousingRequestsController : Controller
    {
        private readonly IHousingRequestService _service;
        public HousingRequestsController(IHousingRequestService service) => _service = service;

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? city, string? university, int page = 1)
        {
            var result = await _service.SearchAsync(city, university, null, null, page, 12);
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var req = await _service.GetByIdWithOffersAsync(id);
            if (req == null || !req.IsActive || req.IsClosed) return NotFound();
            return View(req);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Offer(int id, string message, int? propertyId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var vm = new ViewModels.Student.HousingRequestOfferFormViewModel { Message = message, OfferedPropertyId = propertyId };
            var res = await _service.AddOfferAsync(id, userId, vm);
            TempData[res.Success ? "Success" : "Error"] = res.Success ? "Offer sent" : res.Error;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
