using Microsoft.EntityFrameworkCore;
using StudentHousing.Models;
using StudentHousing.Data;
using StudentHousing.Repositories.Interfaces;

namespace StudentHousing.Repositories.Implementations
{
    public class PropertyRepository : Repository<Property>, IPropertyRepository
    {
        public PropertyRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<Property?> GetByIdWithDetailsAsync(int id)
        {
            return await _db.Properties
                .Include(p => p.Owner.User)
                .Include(p => p.Images)
                .Include(p => p.Rooms)
                .Include(p => p.Amenities)
                .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Approved)).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Property?> GetAdminDetailsAsync(int id)
        {
            return await _db.Properties
                .Include(p => p.Owner.User)
                .Include(p => p.Images)
                .Include(p => p.Amenities)
                .Include(p => p.Rooms).ThenInclude(r => r.Beds)
                .Include(p => p.Reviews).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IReadOnlyList<Property>> GetApprovedActiveAsync()
        {
            return await _db.Properties
                .Include(p => p.Owner.User)
                .Include(p => p.Images)
                .Include(p => p.Rooms)
                .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Approved))
                .Where(p => p.ApprovalStatus == ApprovalStatus.Approved && p.IsActive)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Property>> GetByOwnerAsync(string ownerId)
        {
            return await _db.Properties
                .Include(p => p.Images)
                .Include(p => p.Rooms)
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountByStatusAsync(ApprovalStatus status)
        {
            return await _db.Properties.CountAsync(p => p.ApprovalStatus == status);
        }

        public async Task<IReadOnlyList<Property>> SearchAsync(ApprovalStatus? status, string? search)
        {
            var query = _db.Properties
                .Include(p => p.Owner.User)
                .Include(p => p.Images)
                .Include(p => p.Rooms)
                .AsNoTracking()
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.ApprovalStatus == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(s)
                    || p.City.ToLower().Contains(s)
                    || p.Address.ToLower().Contains(s)
                    || (p.PublicId != null && p.PublicId.ToLower().Contains(s))
                    || p.Owner.User.FirstName.ToLower().Contains(s)
                    || p.Owner.User.LastName.ToLower().Contains(s)
                    || p.Owner.User.Email.ToLower().Contains(s));
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }
    }
}
