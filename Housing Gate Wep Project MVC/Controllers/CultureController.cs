using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace StudentHousing.Controllers
{
    /// <summary>Sets the user's culture and persists it in a cookie.</summary>
    public class CultureController : Controller
    {
        [HttpPost]
        public IActionResult SetLanguage(string culture, string? returnUrl)
        {
            if (new[] { "ar-EG", "en-US" }.Contains(culture, StringComparer.OrdinalIgnoreCase))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax
                    });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
