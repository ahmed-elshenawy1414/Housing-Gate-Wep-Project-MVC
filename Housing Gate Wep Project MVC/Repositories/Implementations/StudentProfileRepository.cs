using Microsoft.EntityFrameworkCore;
using StudentHousing.Models;
using StudentHousing.Data;
using StudentHousing.Repositories.Interfaces;

namespace StudentHousing.Repositories.Implementations
{
    public class StudentProfileRepository : Repository<StudentProfile>, IStudentProfileRepository
    {
        public StudentProfileRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<StudentProfile?> GetByUserIdAsync(string userId)
        {
            return await _db.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Preference)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<StudentProfile?> GetByIdWithDetailsAsync(int id)
        {
            return await _db.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Preference)
                .Include(s => s.Stays.Where(st => st.Status == StayStatus.Completed))
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IReadOnlyList<StudentProfile>> GetAllWithDetailsAsync()
        {
            return await _db.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.Preference)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
