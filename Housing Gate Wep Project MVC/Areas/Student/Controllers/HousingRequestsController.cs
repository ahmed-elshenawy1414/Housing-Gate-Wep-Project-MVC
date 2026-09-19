using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentHousing.Helpers;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = AppRoles.Student)]
    public class HousingRequestsController : Controller
    {
        private readonly IHousingRequestService _service;
        private readonly IUnitOfWork _uow;

        public HousingRequestsController(IHousingRequestService service, IUnitOfWork uow)
        {
            _service = service;
            _uow = uow;
        }

        public async Task<IActionResult> Mine()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var profile = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            if (profile == null) return NotFound();
            var list = await _service.GetMyRequestsAsync(profile.Id);
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new HousingRequestFormViewModel();
            vm.AllAmenities = (await _uow.Amenities.ListAsync(a => a.IsActive)).ToList();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HousingRequestFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllAmenities = (await _uow.Amenities.ListAsync(a => a.IsActive)).ToList();
                return View(model);
            }
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var profile = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            if (profile == null) return NotFound();
            var res = await _service.CreateAsync(model, profile.Id);
            if (!res.Success) { ModelState.AddModelError("", res.Error); model.AllAmenities = (await _uow.Amenities.ListAsync(a => a.IsActive)).ToList(); return View(model); }
            TempData["Success"] = "Request posted";
            return RedirectToAction(nameof(Mine));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var profile = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            var req = await _service.GetByIdAsync(id);
            if (req == null || req.StudentProfileId != profile?.Id) return NotFound();
            var vm = new HousingRequestFormViewModel
            {
                Id = req.Id,
                Title = req.Title,
                Description = req.Description,
                City = req.City,
                District = req.District,
                Governorate = req.Governorate,
                University = req.University,
                PropertyType = req.PropertyType,
                BudgetMin = req.BudgetMin,
                BudgetMax = req.BudgetMax,
                Bedrooms = req.Bedrooms,
                Bathrooms = req.Bathrooms,
                IsFurnished = req.IsFurnished,
                PetAllowed = req.PetAllowed,
                PreferredGender = req.PreferredGender,
                MoveInDate = req.MoveInDate,
                MinLeaseMonths = req.MinLeaseMonths,
                SelectedAmenityIds = req.Amenities.Select(a => a.Id).ToList(),
                AllAmenities = (await _uow.Amenities.ListAsync(a => a.IsActive)).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HousingRequestFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllAmenities = (await _uow.Amenities.ListAsync(a => a.IsActive)).ToList();
                return View(model);
            }
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var profile = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            if (profile == null) return NotFound();
            var res = await _service.UpdateAsync(model, profile.Id);
            if (!res.Success) { ModelState.AddModelError("", res.Error); model.AllAmenities = (await _uow.Amenities.ListAsync(a => a.IsActive)).ToList(); return View(model); }
            TempData["Success"] = "Updated";
            return RedirectToAction(nameof(Mine));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var profile = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            if (profile == null) return NotFound();
            await _service.DeleteAsync(id, profile.Id);
            return RedirectToAction(nameof(Mine));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleClose(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var profile = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            if (profile == null) return NotFound();
            await _service.ToggleCloseAsync(id, profile.Id);
            return RedirectToAction(nameof(Mine));
        }
    }
}
