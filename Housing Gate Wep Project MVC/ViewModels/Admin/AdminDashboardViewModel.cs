using StudentHousing.Models;

namespace StudentHousing.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalOwners { get; set; }
        public int PendingOwnerVerifications { get; set; }
        public int PendingStudentVerifications { get; set; }
        public int PendingProperties { get; set; }
        public int ApprovedProperties { get; set; }
        public int OpenComplaints { get; set; }
        public int PendingReviews { get; set; }
        public int TotalListings { get; set; }

        public List<Property> RecentlyAddedProperties { get; set; } = new List<Property>();
        public List<Complaint> RecentComplaints { get; set; } = new List<Complaint>();

        public List<ApplicationUser> RecentlyJoinedUsers { get; set; } = new List<ApplicationUser>();
        public List<PropertyReview> RecentPendingReviews { get; set; } = new List<PropertyReview>();
    }
}
