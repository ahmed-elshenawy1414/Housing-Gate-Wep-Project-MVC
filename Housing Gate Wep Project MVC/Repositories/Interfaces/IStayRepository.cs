using StudentHousing.Models;

namespace StudentHousing.Repositories.Interfaces
{
    public interface IStayRepository : IRepository<Stay>
    {
        Task<Stay?> GetByIdWithDetailsAsync(int id);

        Task<IReadOnlyList<Stay>> GetByStudentProfileAsync(int studentProfileId);

        Task<IReadOnlyList<Stay>> GetByOwnerAsync(string ownerId);

        /// <summary>
        /// Other students who had a stay in the same property (potential roommates
        /// a student can review after a verified stay).
        /// </summary>
        Task<IReadOnlyList<ApplicationUser>> GetRoommatesInPropertyAsync(int propertyId, string currentUserId);
    }
}
