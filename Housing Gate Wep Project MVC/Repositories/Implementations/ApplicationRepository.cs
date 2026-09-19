using Microsoft.EntityFrameworkCore;
using StudentHousing.Models;
using StudentHousing.Data;
using StudentHousing.Repositories.Interfaces;

namespace StudentHousing.Repositories.Implementations
{
    public class ApplicationRepository : Repository<PropertyApplication>, IApplicationRepository
    {
        public ApplicationRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<PropertyApplication?> GetByIdWithDetailsAsync(int id)
        {
            return await _db.PropertyApplications
                .Include(a => a.Room.Property.Owner.User)
                .Include(a => a.StudentProfile.User)
                .Include(a => a.StudentProfile.Stays)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IReadOnlyList<PropertyApplication>> GetByStudentProfileAsync(int studentProfileId)
        {
            return await _db.PropertyApplications
                .Include(a => a.Room.Property.Images)
                .Include(a => a.Room.Property.Owner.User)
                .Include(a => a.Room)
                .Where(a => a.StudentProfileId == studentProfileId)
                .OrderByDescending(a => a.AppliedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<PropertyApplication>> GetByOwnerAsync(string ownerId)
        {
            return await _db.PropertyApplications
                .Include(a => a.Room.Property.Images)
                .Include(a => a.Room.Property)
                .Include(a => a.StudentProfile).ThenInclude(s => s.User)
                .Where(a => a.Room.Property.OwnerId == ownerId)
                .OrderByDescending(a => a.AppliedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<PropertyApplication>> GetPendingByOwnerAsync(string ownerId)
        {
            return await _db.PropertyApplications
                .Include(a => a.Room.Property.Images)
                .Include(a => a.Room.Property)
                .Include(a => a.StudentProfile).ThenInclude(s => s.User)
                .Where(a => a.Room.Property.OwnerId == ownerId && a.Status == ApplicationStatus.Pending)
                .OrderByDescending(a => a.AppliedAt)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
