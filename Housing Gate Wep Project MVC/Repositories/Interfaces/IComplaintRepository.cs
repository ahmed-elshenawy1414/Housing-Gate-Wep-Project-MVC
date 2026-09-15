using StudentHousing.Models;

namespace StudentHousing.Repositories.Interfaces
{
    public interface IComplaintRepository : IRepository<Complaint>
    {
        Task<Complaint?> GetByIdWithDetailsAsync(int id);

        Task<IReadOnlyList<Complaint>> GetAllWithDetailsAsync(string? search = null);

        Task<IReadOnlyList<Complaint>> GetByComplainantAsync(string userId);

        Task<IReadOnlyList<Complaint>> GetByPropertyOwnerAsync(string ownerId);

        Task<int> CountByStatusAsync(ComplaintStatus status);
    }
}
