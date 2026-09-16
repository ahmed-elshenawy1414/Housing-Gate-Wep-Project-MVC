using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentHousing.Areas.Admin.ViewModels;
using StudentHousing.Models;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISiteSettingService _siteSettings;
        private readonly IConfiguration _config;

        public SettingsController(UserManager<ApplicationUser> userManager, ISiteSettingService siteSettings, IConfiguration config)
        {
            _userManager = userManager;
            _siteSettings = siteSettings;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var all = await _siteSettings.GetAllAsync();
            var model = new AdminSettingsViewModel
            {
                FacebookUrl = user.FacebookUrl ?? "",
                InstagramUrl = user.InstagramUrl ?? "",
                TikTokUrl = user.TikTokUrl ?? "",
                HomeHeroTitle = all.GetValueOrDefault("Home.Hero", ""),
                HomeHeroSub = all.GetValueOrDefault("Home.HeroSub", ""),
                AboutLead = all.GetValueOrDefault("About.Lead", ""),
                AboutBody = all.GetValueOrDefault("About.Body", ""),
                HowItWorksStep1 = all.GetValueOrDefault("HiW.Step1Text", ""),
                HowItWorksStep2 = all.GetValueOrDefault("HiW.Step2Text", ""),
                HowItWorksStep3 = all.GetValueOrDefault("HiW.Step3Text", "")
            };
            // Fallback to config/resources if empty
            if (string.IsNullOrWhiteSpace(model.HomeHeroTitle)) model.HomeHeroTitle = _config["SiteDefaults:HomeHero"] ?? "";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AdminSettingsViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FacebookUrl = model.FacebookUrl;
            user.InstagramUrl = model.InstagramUrl;
            user.TikTokUrl = model.TikTokUrl;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _siteSettings.SetAsync("Home.Hero", model.HomeHeroTitle ?? "");
            await _siteSettings.SetAsync("Home.HeroSub", model.HomeHeroSub ?? "");
            await _siteSettings.SetAsync("About.Lead", model.AboutLead ?? "");
            await _siteSettings.SetAsync("About.Body", model.AboutBody ?? "");
            await _siteSettings.SetAsync("HiW.Step1Text", model.HowItWorksStep1 ?? "");
            await _siteSettings.SetAsync("HiW.Step2Text", model.HowItWorksStep2 ?? "");
            await _siteSettings.SetAsync("HiW.Step3Text", model.HowItWorksStep3 ?? "");

            TempData["Success"] = "Settings updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}