using Microsoft.EntityFrameworkCore;
using StudentHousing.Models;
using StudentHousing.Data;
using StudentHousing.Repositories.Interfaces;

namespace StudentHousing.Repositories.Implementations
{
    public class ReviewRepository : Repository<PropertyReview>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<IReadOnlyList<PropertyReview>> GetApprovedForPropertyAsync(int propertyId)
        {
            return await _db.PropertyReviews
                .Include(r => r.Reviewer)
                .Where(r => r.PropertyId == propertyId && r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<PropertyReview>> GetPendingAsync()
        {
            return await _db.PropertyReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Property)
                .Where(r => r.Status == ReviewStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<PropertyReview>> GetByReviewerAsync(string reviewerUserId)
        {
            return await _db.PropertyReviews
                .Include(r => r.Property.Images)
                .Include(r => r.Property.Rooms)
                .Where(r => r.ReviewerId == reviewerUserId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<PropertyReview>> GetByPropertyOwnerAsync(string ownerId)
        {
            return await _db.PropertyReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Property)
                .Where(r => r.Property.OwnerId == ownerId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PropertyReview?> GetByIdWithDetailsAsync(int id)
        {
            return await _db.PropertyReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Property.Owner.User)
                .Include(r => r.Stay)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> AlreadyReviewedStayAsync(int stayId, string reviewerUserId)
        {
            return await _db.PropertyReviews.AnyAsync(r => r.StayId == stayId && r.ReviewerId == reviewerUserId);
        }

        public async Task AddUserReviewAsync(UserReview review)
        {
            await _db.UserReviews.AddAsync(review);
        }

        public async Task<bool> UserReviewExistsAsync(int stayId, string reviewerId, string reviewedUserId)
        {
            return await _db.UserReviews.AnyAsync(r =>
                r.StayId == stayId && r.ReviewerId == reviewerId && r.ReviewedUserId == reviewedUserId);
        }
    }
}
