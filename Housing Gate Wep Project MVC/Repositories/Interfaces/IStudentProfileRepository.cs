using StudentHousing.Models;

namespace StudentHousing.Repositories.Interfaces
{
    public interface IStudentProfileRepository : IRepository<StudentProfile>
    {
        Task<StudentProfile?> GetByUserIdAsync(string userId);

        Task<StudentProfile?> GetByIdWithDetailsAsync(int id);

        /// <summary>Profiles with user + preference loaded, used by the matching engine.</summary>
        Task<IReadOnlyList<StudentProfile>> GetAllWithDetailsAsync();
    }
}
