using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Owner;

namespace StudentHousing.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = AppRoles.Owner)]
    public class ProfileController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifications;
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<SharedResource> _L;
        private readonly StudentHousing.Services.Interfaces.IDocumentStorageService _docs;

        public ProfileController(IUnitOfWork uow, INotificationService notifications, IWebHostEnvironment env, IStringLocalizer<SharedResource> L, StudentHousing.Services.Interfaces.IDocumentStorageService docs)
        {
            _uow = uow;
            _notifications = notifications;
            _env = env;
            _L = L;
            _docs = docs;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _uow.OwnerProfiles.FirstOrDefaultAsync(o => o.UserId == userId, includeProperties: "User");
            if (profile == null)
            {
                return NotFound();
            }

            return View(ToViewModel(profile));
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _uow.OwnerProfiles.FirstOrDefaultAsync(o => o.UserId == userId, includeProperties: "User");
            if (profile == null)
            {
                return NotFound();
            }

            return View(ToViewModel(profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OwnerProfileViewModel model, IFormFile? profilePicture)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _uow.OwnerProfiles.FirstOrDefaultAsync(o => o.UserId == userId, includeProperties: "User");
            if (profile == null)
            {
                return NotFound();
            }

            profile.CompanyName = model.CompanyName;
            profile.Bio = model.Bio;
            profile.Phone = model.Phone;
            profile.LicenseNumber = model.LicenseNumber;

            if (profilePicture is { Length: > 0 })
            {
                var path = await ImageFileHelper.SaveAsync(profilePicture, _env);
                if (!string.IsNullOrEmpty(profile.User?.ProfilePictureUrl))
                {
                    ImageFileHelper.Delete(profile.User.ProfilePictureUrl, _env);
                }
                if (profile.User != null)
                {
                    profile.User.ProfilePictureUrl = path;
                }
            }

            await _uow.SaveChangesAsync();

            TempData["Success"] = _L["Msg.ProfileUpdated"].ToString();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Verify() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(IFormFile document)
        {
            if (document == null || document.Length == 0)
            {
                TempData["Error"] = _L["Msg.AttachDocument"].ToString();
                return View();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _uow.OwnerProfiles.FirstOrDefaultAsync(o => o.UserId == userId, includeProperties: "User");
            if (profile == null)
            {
                return NotFound();
            }

            var validationError = ImageFileHelper.Validate(document, _L);
            if (validationError != null)
            {
                TempData["Error"] = validationError;
                return View();
            }

            var res = await _docs.SavePrivateAsync(document, userId, "owner-verify");
            if (!res.Success)
            {
                TempData["Error"] = res.Error;
                return View();
            }
            string path = res.PrivatePath!;

            _docs.DeletePrivate(profile.VerificationDocumentUrl);
            if (!string.IsNullOrEmpty(profile.VerificationDocumentUrl) && profile.VerificationDocumentUrl.StartsWith("/uploads/"))
                ImageFileHelper.Delete(profile.VerificationDocumentUrl, _env);

            profile.VerificationDocumentUrl = path;
            profile.VerificationStatus = VerificationStatus.Pending;
            await _uow.SaveChangesAsync();

            TempData["Success"] = _L["Msg.DocsUploaded"].ToString();
            return RedirectToAction(nameof(Index));
        }

        private static OwnerProfileViewModel ToViewModel(OwnerProfile profile)
        {
            return new OwnerProfileViewModel
            {
                CompanyName = profile.CompanyName,
                Bio = profile.Bio,
                Phone = profile.Phone,
                LicenseNumber = profile.LicenseNumber,
                VerificationStatus = profile.VerificationStatus,
                VerifiedAt = profile.VerifiedAt,
                HasVerificationDocument = !string.IsNullOrEmpty(profile.VerificationDocumentUrl),
                Email = profile.User?.Email ?? string.Empty,
                FirstName = profile.User?.FirstName ?? string.Empty,
                LastName = profile.User?.LastName ?? string.Empty,
                ProfilePictureUrl = profile.User?.ProfilePictureUrl
            };
        }
    }
}
