using StudentHousing.Models;

namespace StudentHousing.ViewModels.Student
{
    public class PropertySearchViewModel
    {
        public string? Keyword { get; set; }
        public string? City { get; set; }
        public PropertyType? PropertyType { get; set; }
        public int? MaxRent { get; set; }
        public string? SortBy { get; set; } // "price-asc" | "price-desc" | "newest"

        // Pagination (Phase 4)
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public List<PropertyCardViewModel> Results { get; set; } = new List<PropertyCardViewModel>();
        public List<string> Cities { get; set; } = new List<string>();
    }
}
