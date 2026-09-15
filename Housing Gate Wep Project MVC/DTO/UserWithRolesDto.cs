using StudentHousing.Models;

namespace StudentHousing.DTOs
{
    public class UserWithRolesDto
    {
        public ApplicationUser User { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
