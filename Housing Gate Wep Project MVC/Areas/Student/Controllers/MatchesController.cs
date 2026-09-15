using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentHousing.Helpers;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = AppRoles.Student)]
    public class MatchesController : Controller
    {
        private readonly IMatchingService _matchingService;
        private readonly IStudentService _studentService;

        public MatchesController(IMatchingService matchingService, IStudentService studentService)
        {
            _matchingService = matchingService;
            _studentService = studentService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var profile = await _studentService.GetMyProfileAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            var model = new MatchesViewModel
            {
                ProfileIncomplete = profile.Preference == null || profile.BudgetMax <= 0,
                Matches = (await _matchingService.GetMatchesAsync(profile.Id)).ToList()
            };

            return View(model);
        }
    }
}
