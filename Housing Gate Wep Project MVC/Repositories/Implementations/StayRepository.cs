using Microsoft.EntityFrameworkCore;
using StudentHousing.Models;
using StudentHousing.Data;
using StudentHousing.Repositories.Interfaces;

namespace StudentHousing.Repositories.Implementations
{
    public class StayRepository : Repository<Stay>, IStayRepository
    {
        public StayRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<Stay?> GetByIdWithDetailsAsync(int id)
        {
            return await _db.Stays
                .Include(s => s.Room.Property.Owner.User)
                .Include(s => s.Room)
                .Include(s => s.StudentProfile.User)
                .Include(s => s.UserReviews)
                .Include(s => s.PropertyReviews)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IReadOnlyList<Stay>> GetByStudentProfileAsync(int studentProfileId)
        {
            return await _db.Stays
                .Include(s => s.Room.Property.Images)
                .Include(s => s.Room.Property.Owner.User)
                .Include(s => s.Room)
                .Include(s => s.StudentProfile.User)
                .Where(s => s.StudentProfileId == studentProfileId)
                .OrderByDescending(s => s.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Stay>> GetByOwnerAsync(string ownerId)
        {
            return await _db.Stays
                .Include(s => s.Room.Property.Images)
                .Include(s => s.Room)
                .Include(s => s.StudentProfile.User)
                .Where(s => s.Room.Property.OwnerId == ownerId)
                .OrderByDescending(s => s.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetRoommatesInPropertyAsync(int propertyId, string currentUserId)
        {
            var roommateUserIds = await _db.Stays
                .Include(s => s.Room)
                .Where(s => s.Room.PropertyId == propertyId
                            && s.StudentProfile.UserId != currentUserId
                            && s.StudentProfile.User.IsActive)
                .Select(s => s.StudentProfile.UserId)
                .Distinct()
                .ToListAsync();

            return await _db.Users
                .Where(u => roommateUserIds.Contains(u.Id))
                .ToListAsync();
        }
    }
}
