using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;

        public NotificationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task CreateAsync(string userId, string title, string message, string? link = null)
        {
            await _uow.Notifications.AddAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link
            });
            await _uow.SaveChangesAsync();
        }

        public async Task CreateManyAsync(IEnumerable<(string userId, string title, string message, string? link)> items)
        {
            var list = items.Select(i => new Notification { UserId = i.userId, Title = i.title, Message = i.message, Link = i.link }).ToList();
            if (list.Count == 0) return;
            foreach (var n in list) await _uow.Notifications.AddAsync(n);
            await _uow.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Notification>> GetRecentAsync(string userId, int count = 20)
            => await _uow.Notifications.GetRecentForUserAsync(userId, count);

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _uow.Notifications.GetUnreadCountAsync(userId);

        public async Task MarkAllAsReadAsync(string userId)
            => await _uow.Notifications.MarkAllAsReadAsync(userId);
    }
}
