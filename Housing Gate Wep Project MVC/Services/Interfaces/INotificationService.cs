using StudentHousing.Models;

namespace StudentHousing.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(string userId, string title, string message, string? link = null);

        Task<IReadOnlyList<Notification>> GetRecentAsync(string userId, int count = 20);

        Task<int> GetUnreadCountAsync(string userId);

        Task MarkAllAsReadAsync(string userId);
    }
}
