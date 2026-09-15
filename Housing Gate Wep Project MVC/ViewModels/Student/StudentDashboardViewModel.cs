using StudentHousing.Models;

namespace StudentHousing.ViewModels.Student
{
    /// <summary>Overview shown on the student dashboard.</summary>
    public class StudentDashboardViewModel
    {
        public StudentProfile Profile { get; set; } = null!;
        public int ActiveApplications { get; set; }
        public int ActiveStays { get; set; }
        public int CompletedStays { get; set; }
        public int PendingReviews { get; set; }
        public List<PropertyApplication> RecentApplications { get; set; } = new List<PropertyApplication>();
        public List<Stay> RecentStays { get; set; } = new List<Stay>();
    }
}
