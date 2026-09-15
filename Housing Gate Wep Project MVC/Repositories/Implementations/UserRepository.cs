using Microsoft.EntityFrameworkCore;
using StudentHousing.DTOs;
using StudentHousing.Models;
using StudentHousing.Data;
using StudentHousing.Repositories.Interfaces;

namespace StudentHousing.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ApplicationUser?> GetUserWithProfileAsync(string userId)
        {
            return await _db.Users
                .Include(u => u.OwnerProfile)
                .Include(u => u.StudentProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetRecentUsersAsync(int take = 5)
        {
            return await _db.Users
                .Include(u => u.OwnerProfile)
                .Include(u => u.StudentProfile)
                .OrderByDescending(u => u.CreatedAt)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<UserWithRolesDto>> GetAllUsersWithRolesAsync(string? search = null)
        {
            var query =
                from user in _db.Users
                join userRole in _db.UserRoles on user.Id equals userRole.UserId
                join role in _db.Roles on userRole.RoleId equals role.Id
                where string.IsNullOrWhiteSpace(search)
                      || user.FirstName.ToLower().Contains(search.ToLower())
                      || user.LastName.ToLower().Contains(search.ToLower())
                      || user.Email.ToLower().Contains(search.ToLower())
                      || role.Name.ToLower().Contains(search.ToLower())
                group role.Name by user into grouped
                select new UserWithRolesDto
                {
                    User = grouped.Key,
                    Roles = grouped.ToList()
                };

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetOwnersWithProfilesAsync(string? search = null)
        {
            return await _db.Users
                .Include(u => u.OwnerProfile)
                .Where(u => u.OwnerProfile != null
                    && (string.IsNullOrWhiteSpace(search)
                        || u.FirstName.ToLower().Contains(search.ToLower())
                        || u.LastName.ToLower().Contains(search.ToLower())
                        || u.Email.ToLower().Contains(search.ToLower())
                        || (u.OwnerProfile.CompanyName != null && u.OwnerProfile.CompanyName.ToLower().Contains(search.ToLower()))))
                .OrderBy(u => u.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetStudentsWithProfilesAsync(string? search = null)
        {
            return await _db.Users
                .Include(u => u.StudentProfile)
                .Where(u => u.StudentProfile != null
                    && (string.IsNullOrWhiteSpace(search)
                        || u.FirstName.ToLower().Contains(search.ToLower())
                        || u.LastName.ToLower().Contains(search.ToLower())
                        || u.Email.ToLower().Contains(search.ToLower())
                        || u.StudentProfile.University.ToLower().Contains(search.ToLower())))
                .OrderBy(u => u.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<UserReview>> GetReviewsAboutUserAsync(string userId)
        {
            return await _db.UserReviews
                .Where(r => r.ReviewedUserId == userId && r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<UserReview>> GetReviewsByUserAsync(string userId)
        {
            return await _db.UserReviews
                .Where(r => r.ReviewerId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
