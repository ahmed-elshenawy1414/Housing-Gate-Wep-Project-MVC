using StudentHousing.Models;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Interfaces
{
    public interface IComplaintService
    {
        Task<(bool Success, string Error)> FileAsync(ComplaintFormViewModel model, string complainantId);

        Task<IReadOnlyList<Complaint>> GetByComplainantAsync(string userId);

        Task<IReadOnlyList<Complaint>> GetAllAsync(string? search = null);

        Task<Complaint?> GetByIdAsync(int id);

        Task<(bool Success, string Error)> RespondAsync(int complaintId, ComplaintStatus status, string? response);
    }
}
