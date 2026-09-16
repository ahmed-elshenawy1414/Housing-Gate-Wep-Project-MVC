using Microsoft.EntityFrameworkCore;
using StudentHousing.Models;
using StudentHousing.Data;
using StudentHousing.Repositories.Interfaces;

namespace StudentHousing.Repositories.Implementations
{
    public class ComplaintRepository : Repository<Complaint>, IComplaintRepository
    {
        public ComplaintRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<Complaint?> GetByIdWithDetailsAsync(int id)
        {
            return await _db.Complaints
                .Include(c => c.Complainant)
                .Include(c => c.TargetUser)
                .Include(c => c.TargetProperty)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IReadOnlyList<Complaint>> GetAllWithDetailsAsync(string? search = null)
        {
            var query = _db.Complaints
                .Include(c => c.Complainant)
                .Include(c => c.TargetUser)
                .Include(c => c.TargetProperty)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Subject.ToLower().Contains(s)
                    || c.Description.ToLower().Contains(s)
                    || c.Complainant.FirstName.ToLower().Contains(s)
                    || c.Complainant.LastName.ToLower().Contains(s)
                    || (c.Complainant.Email != null && c.Complainant.Email.ToLower().Contains(s))
                    || (c.TargetProperty != null && c.TargetProperty.Title.ToLower().Contains(s)));
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<IReadOnlyList<Complaint>> GetByComplainantAsync(string userId)
        {
            return await _db.Complaints
                .Include(c => c.TargetUser)
                .Include(c => c.TargetProperty)
                .Where(c => c.ComplainantId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Complaint>> GetByPropertyOwnerAsync(string ownerId)
        {
            return await _db.Complaints
                .Include(c => c.Complainant)
                .Include(c => c.TargetProperty)
                .Where(c => c.TargetProperty != null && c.TargetProperty.OwnerId == ownerId)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountByStatusAsync(ComplaintStatus status)
        {
            return await _db.Complaints.CountAsync(c => c.Status == status);
        }
    }
}
