using StudentHousing.Models;

namespace StudentHousing.Repositories.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IReadOnlyList<Notification>> GetRecentForUserAsync(string userId, int count = 20);

        Task<int> GetUnreadCountAsync(string userId);

        Task MarkAllAsReadAsync(string userId);
    }
}
