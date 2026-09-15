using StudentHousing.Models;

namespace StudentHousing.ViewModels.Owner
{
    public class OwnerDashboardViewModel
    {
        public OwnerProfile Profile { get; set; } = null!;
        public int TotalProperties { get; set; }
        public int ApprovedProperties { get; set; }
        public int PendingApplications { get; set; }
        public int ActiveStays { get; set; }
        public int OpenComplaints { get; set; }
        public int PendingReviews { get; set; }
        public List<Property> Properties { get; set; } = new List<Property>();
        public List<PropertyApplication> RecentApplications { get; set; } = new List<PropertyApplication>();
        public List<Complaint> RecentComplaints { get; set; } = new List<Complaint>();
        public List<PropertyReview> RecentReviews { get; set; } = new List<PropertyReview>();
    }
}
