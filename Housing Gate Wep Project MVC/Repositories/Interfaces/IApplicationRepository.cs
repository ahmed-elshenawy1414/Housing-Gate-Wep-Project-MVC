using StudentHousing.Models;

namespace StudentHousing.Repositories.Interfaces
{
    public interface IApplicationRepository : IRepository<PropertyApplication>
    {
        Task<PropertyApplication?> GetByIdWithDetailsAsync(int id);

        Task<IReadOnlyList<PropertyApplication>> GetByStudentProfileAsync(int studentProfileId);

        Task<IReadOnlyList<PropertyApplication>> GetByOwnerAsync(string ownerId);

        Task<IReadOnlyList<PropertyApplication>> GetPendingByOwnerAsync(string ownerId);
    }
}
