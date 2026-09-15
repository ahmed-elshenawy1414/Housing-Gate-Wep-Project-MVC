using StudentHousing.ViewModels.Owner;

namespace StudentHousing.Services.Interfaces
{
    public interface IOwnerService
    {
        Task<OwnerDashboardViewModel> GetDashboardAsync(string ownerId);
    }
}
