using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Mappings;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = AppRoles.Student)]
    public class SearchController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IStudentService _studentService;
        private readonly IUnitOfWork _uow;
        private readonly IStringLocalizer<SharedResource> _L;

        public SearchController(IPropertyService propertyService, IStudentService studentService, IUnitOfWork uow, IStringLocalizer<SharedResource> L)
        {
            _propertyService = propertyService;
            _studentService = studentService;
            _uow = uow;
            _L = L;
        }

        [HttpGet]
        public async Task<IActionResult> Index(PropertySearchViewModel search, int page = 1, int pageSize = 12)
        {
            search.Page = page;
            search.PageSize = Math.Clamp(pageSize, 1, 50);
            search.Cities = (await _propertyService.GetCitiesAsync()).ToList();
            search.Results = (await _propertyService.SearchAsync(search)).ToList();
            return View(search);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyService.GetByIdWithDetailsAsync(id);
            if (property == null || property.ApprovalStatus != ApprovalStatus.Approved || !property.IsActive)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _studentService.GetMyProfileAsync(userId);

            var model = new PropertyDetailViewModel
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                PropertyType = property.PropertyType,
                Address = property.Address,
                City = property.City,
                State = property.State,
                ZipCode = property.ZipCode,
                Deposit = property.Deposit,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                IsFurnished = property.IsFurnished,
                PetAllowed = property.PetAllowed,
                AvailableFrom = property.AvailableFrom,
                IsFeatured = property.IsFeatured,
                Images = property.Images.ToList(),
                Rooms = property.Rooms.ToList(),
                Reviews = property.Reviews.ToList(),
                AverageRating = property.Reviews.Count == 0 ? 0 : Math.Round(property.Reviews.Average(r => r.Rating), 1),
                OwnerName = property.Owner?.User?.FullName ?? _L["Account.Owner"],
                OwnerIsVerified = property.Owner?.VerificationStatus == VerificationStatus.Verified
            };

            if (profile != null)
            {
                var myApplication = await _uow.Applications.FirstOrDefaultAsync(a =>
                    a.StudentProfileId == profile.Id
                    && a.Room.PropertyId == property.Id
                    && a.Status != ApplicationStatus.Cancelled,
                    includeProperties: "Room");

                model.AlreadyAppliedRoomId = myApplication?.RoomId;
                model.AppliedMessage = myApplication?.Message;
                model.AppliedStatus = myApplication == null ? null : (int)myApplication.Status;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Details), new { id = model.PropertyId });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _studentService.GetMyProfileAsync(userId);
            if (profile == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var result = await _propertyService.ApplyAsync(model.RoomId, profile.Id, model.Message);

            if (result.Success)
            {
                TempData["Success"] = _L["Msg.ApplicationSent"].ToString();
            }
            else
            {
                TempData["Error"] = result.Error;
            }

            return RedirectToAction(nameof(Details), new { id = model.PropertyId });
        }
    }
}
