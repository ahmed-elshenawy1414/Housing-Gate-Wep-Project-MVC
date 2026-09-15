using StudentHousing.Models;

namespace StudentHousing.ViewModels
{
    public class HomeViewModel
    {
        public List<PropertyCardViewModel> FeaturedProperties { get; set; } = new List<PropertyCardViewModel>();
        public int AvailableListings { get; set; }
        public int VerifiedStudents { get; set; }
        public int VerifiedOwners { get; set; }
        public List<string> PopularCities { get; set; } = new List<string>();
    }
}
