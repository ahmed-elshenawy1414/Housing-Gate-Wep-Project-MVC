using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels;

namespace StudentHousing.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IPropertyService propertyService, IUnitOfWork uow, ILogger<HomeController> logger)
        {
            _propertyService = propertyService;
            _uow = uow;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _uow.Users.GetStudentsWithProfilesAsync();
            var owners = await _uow.Users.GetOwnersWithProfilesAsync();

            var model = new HomeViewModel
            {
                FeaturedProperties = (await _propertyService.GetForHomeAsync(6)).ToList(),
                AvailableListings = await _propertyService.CountApprovedActiveAsync(),
                VerifiedStudents = students.Count(s => s.StudentProfile != null && s.StudentProfile.VerificationStatus == VerificationStatus.Verified),
                VerifiedOwners = owners.Count(o => o.OwnerProfile != null && o.OwnerProfile.VerificationStatus == VerificationStatus.Verified),
                PopularCities = (await _propertyService.GetCitiesAsync()).ToList()
            };

            return View(model);
        }

        public IActionResult HowItWorks() => View();

        public IActionResult About() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
