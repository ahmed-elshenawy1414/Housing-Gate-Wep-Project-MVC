using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Owner;

namespace StudentHousing.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = AppRoles.Owner)]
    public class PropertiesController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IStringLocalizer<SharedResource> _L;

        public PropertiesController(IPropertyService propertyService, IStringLocalizer<SharedResource> L)
        {
            _propertyService = propertyService;
            _L = L;
        }

        public async Task<IActionResult> Index()
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var properties = await _propertyService.GetByOwnerAsync(ownerId);
            return View(properties);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new PropertyFormViewModel
            {
                AllAmenities = (await _propertyService.GetActiveAmenitiesAsync()).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllAmenities = (await _propertyService.GetActiveAmenitiesAsync()).ToList();
                return View(model);
            }

            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _propertyService.CreateAsync(model, ownerId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                model.AllAmenities = (await _propertyService.GetActiveAmenitiesAsync()).ToList();
                return View(model);
            }

            TempData["Success"] = _L["Msg.ListingCreated"].ToString();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var property = await _propertyService.GetByIdWithDetailsAsync(id);
            if (property == null || property.OwnerId != ownerId)
            {
                return NotFound();
            }

            var model = new PropertyFormViewModel
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                PropertyType = property.PropertyType,
                Address = property.Address,
                City = property.City,
                State = property.State,
                District = property.District,
                ZipCode = property.ZipCode,
                University = property.University,
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                Deposit = property.Deposit,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                IsFurnished = property.IsFurnished,
                PetAllowed = property.PetAllowed,
                AvailableFrom = property.AvailableFrom,
                AllowedGender = property.AllowedGender,
                ExistingImages = property.Images.ToList(),
                ApprovalStatusDisplay = property.ApprovalStatus.ToString(),
                SelectedAmenityIds = property.Amenities.Select(a => a.Id).ToList(),
                AllAmenities = (await _propertyService.GetActiveAmenitiesAsync()).ToList(),
                Rooms = property.Rooms.Select(r => new RoomFormViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    RoomType = r.RoomType,
                    RentPerMonth = r.RentPerMonth,
                    Description = r.Description,
                    BathroomType = r.BathroomType,
                    NumberOfBeds = r.NumberOfBeds,
                    AvailableBeds = r.AvailableBeds,
                    IsAvailable = r.IsAvailable
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PropertyFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllAmenities = (await _propertyService.GetActiveAmenitiesAsync()).ToList();
                return View(model);
            }

            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _propertyService.UpdateAsync(model, ownerId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                model.AllAmenities = (await _propertyService.GetActiveAmenitiesAsync()).ToList();
                return View(model);
            }

            TempData["Success"] = _L["Msg.ListingUpdated"].ToString();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            await _propertyService.ToggleActiveAsync(id, ownerId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _propertyService.DeleteAsync(id, ownerId);
            TempData[result.Success ? "Success" : "Error"] = result.Success ? _L["Msg.ListingDeleted"] : result.Error;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int propertyId, int imageId)
        {
            var ownerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            await _propertyService.DeleteImageAsync(propertyId, imageId, ownerId);
            return RedirectToAction(nameof(Edit), new { id = propertyId });
        }
    }
}
