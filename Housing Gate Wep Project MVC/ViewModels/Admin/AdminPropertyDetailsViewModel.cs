using StudentHousing.Models;

namespace StudentHousing.ViewModels.Admin
{
    public class AdminPropertyDetailsViewModel
    {
        public Property Property { get; set; } = null!;

        public int ApplicationCount { get; set; }

        public int StayCount { get; set; }

        public int ComplaintCount { get; set; }

        public bool CanDelete { get; set; }

        public List<string> BlockReasons { get; set; } = new List<string>();
    }
}
