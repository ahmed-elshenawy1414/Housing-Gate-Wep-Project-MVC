using StudentHousing.Models;

namespace StudentHousing.ViewModels.Admin
{
    public class AdminUserDetailsViewModel
    {
        public ApplicationUser User { get; set; } = null!;

        public List<string> Roles { get; set; } = new List<string>();

        public OwnerProfile? OwnerProfile { get; set; }

        public StudentProfile? StudentProfile { get; set; }

        public int PropertyCount { get; set; }

        public int ApplicationCount { get; set; }

        public int StayCount { get; set; }

        public int ComplaintsFiled { get; set; }

        public int ComplaintsReceived { get; set; }

        public int UserReviewsGiven { get; set; }

        public int UserReviewsReceived { get; set; }

        public bool CanDelete { get; set; }

        public List<string> BlockReasons { get; set; } = new List<string>();
    }
}
