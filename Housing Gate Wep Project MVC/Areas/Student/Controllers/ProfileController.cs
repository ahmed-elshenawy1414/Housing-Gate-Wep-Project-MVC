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
    public class ProfileController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<SharedResource> _L;

        public ProfileController(IStudentService studentService, IUnitOfWork uow, IWebHostEnvironment env, IStringLocalizer<SharedResource> L)
        {
            _studentService = studentService;
            _uow = uow;
            _env = env;
            _L = L;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _studentService.GetMyProfileAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            return View(StudentMapper.ToProfileViewModel(profile));
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _studentService.GetMyProfileAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            return View(StudentMapper.ToProfileViewModel(profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentProfileViewModel model, IFormFile? profilePicture)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _studentService.UpdateProfileAsync(userId, model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return View(model);
            }

            if (profilePicture is { Length: > 0 })
            {
                var path = await ImageFileHelper.SaveAsync(profilePicture, _env);
                var profile = await _uow.StudentProfiles.GetByUserIdAsync(userId);
                if (profile?.User != null)
                {
                    if (!string.IsNullOrEmpty(profile.User.ProfilePictureUrl))
                    {
                        ImageFileHelper.Delete(profile.User.ProfilePictureUrl, _env);
                    }
                    profile.User.ProfilePictureUrl = path;
                    await _uow.SaveChangesAsync();
                }
            }

            TempData["Success"] = _L["Msg.ProfileUpdated"] + " " + _L["Msg.UpdatePrefs"];
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Preferences()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _studentService.GetMyProfileAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            return View(StudentMapper.ToPreferenceViewModel(profile.Preference));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preferences(RoommatePreferenceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _studentService.UpdatePreferenceAsync(userId, model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return View(model);
            }

            TempData["Success"] = _L["Msg.PrefsSaved"].ToString();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Verify() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(IFormFile nationalId, IFormFile universityId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _studentService.SubmitVerificationAsync(userId, nationalId, universityId);

            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return View();
            }

            TempData["Success"] = _L["Msg.DocsUploaded"].ToString();
            return RedirectToAction(nameof(Index));
        }
    }
}
