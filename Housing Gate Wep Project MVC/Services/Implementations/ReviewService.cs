using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifications;
        private readonly IAuditLogService _auditLog;
        private readonly IStringLocalizer<SharedResource> _L;
        private readonly IWebHostEnvironment _env;

        public ReviewService(IUnitOfWork uow, INotificationService notifications, IAuditLogService auditLog,
            IStringLocalizer<SharedResource> L, IWebHostEnvironment env)
        {
            _uow = uow;
            _notifications = notifications;
            _auditLog = auditLog;
            _L = L;
            _env = env;
        }

        public async Task<IReadOnlyList<PropertyReview>> GetApprovedForPropertyAsync(int propertyId)
            => await _uow.Reviews.GetApprovedForPropertyAsync(propertyId);

        public async Task<IReadOnlyList<PropertyReview>> GetByReviewerAsync(string reviewerUserId)
            => await _uow.Reviews.GetByReviewerAsync(reviewerUserId);

        public async Task<(bool Success, string Error)> AddPropertyReviewAsync(
            PropertyReviewFormViewModel model, string reviewerUserId)
        {
            var stay = await _uow.Stays.GetByIdWithDetailsAsync(model.StayId);
            if (stay == null || stay.StudentProfile.UserId != reviewerUserId)
            {
                return (false, _L["Err.StayNotYours"]);
            }

            if (stay.Status != StayStatus.Completed)
            {
                return (false, _L["Err.ReviewAfterStayEnd"]);
            }

            if (await _uow.Reviews.AlreadyReviewedStayAsync(model.StayId, reviewerUserId))
            {
                return (false, _L["Err.AlreadyReviewedProperty"]);
            }

            // Validate photos (performance: max 3, security: extension/MIME/magic)
            if (model.Photos != null && model.Photos.Count > 3)
                return (false, _L["Err.TooManyPhotos"]);

            string? photoError = null;
            if (model.Photos != null)
            {
                foreach (var f in model.Photos.Where(f => f != null && f.Length > 0))
                {
                    photoError = ImageFileHelper.Validate(f, _L);
                    if (photoError != null) break;
                }
            }
            if (photoError != null) return (false, photoError);

            var review = new PropertyReview
            {
                PropertyId = model.PropertyId,
                StayId = model.StayId,
                ReviewerId = reviewerUserId,
                Rating = model.Rating,
                Title = model.Title,
                Comment = model.Comment,
                Status = ReviewStatus.Pending
            };

            await _uow.Reviews.AddAsync(review);
            await _uow.SaveChangesAsync();

            // Save photos after review has Id (DB integrity: FK)
            if (model.Photos != null)
            {
                foreach (var f in model.Photos.Where(f => f != null && f.Length > 0).Take(3))
                {
                    var path = await ImageFileHelper.SaveAsync(f, _env);
                    // Store under review-specific subfolder for privacy? Keep public for listing visibility
                    review.Images.Add(new PropertyReviewImage { PropertyReviewId = review.Id, FilePath = path });
                }
                await _uow.SaveChangesAsync();
            }
            return (true, string.Empty);
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetRoommatesAsync(int stayId, string currentUserId)
        {
            var stay = await _uow.Stays.GetByIdWithDetailsAsync(stayId);
            if (stay == null || stay.StudentProfile.UserId != currentUserId)
            {
                return new List<ApplicationUser>();
            }

            var propertyId = stay.Room.PropertyId;
            return await _uow.Stays.GetRoommatesInPropertyAsync(propertyId, currentUserId);
        }

        public async Task<(bool Success, string Error)> AddUserReviewAsync(
            int stayId, string reviewedUserId, string reviewerId, int rating, string comment)
        {
            if (reviewedUserId == reviewerId)
            {
                return (false, _L["Err.CannotReviewSelf"]);
            }

            var stay = await _uow.Stays.GetByIdWithDetailsAsync(stayId);
            if (stay == null || stay.StudentProfile.UserId != reviewerId)
            {
                return (false, _L["Err.StayNotYours"]);
            }

            if (stay.Status != StayStatus.Completed)
            {
                return (false, _L["Err.ReviewRoommatesAfterStayEnd"]);
            }

            // The reviewed person must have had a stay in the same property.
            var roommates = await _uow.Stays.GetRoommatesInPropertyAsync(stay.Room.PropertyId, reviewerId);
            if (roommates.All(r => r.Id != reviewedUserId))
            {
                return (false, _L["Err.OnlyLivedTogether"]);
            }

            if (await _uow.Reviews.UserReviewExistsAsync(stayId, reviewerId, reviewedUserId))
            {
                return (false, _L["Err.AlreadyReviewedRoommate"]);
            }

            var review = new UserReview
            {
                StayId = stayId,
                ReviewerId = reviewerId,
                ReviewedUserId = reviewedUserId,
                Rating = rating,
                Comment = comment,
                Status = ReviewStatus.Pending
            };

            await _uow.Reviews.AddUserReviewAsync(review);
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> AddOwnerToStudentReviewAsync(int stayId, string reviewedStudentUserId, string ownerUserId, int rating, string comment)
        {
            if (reviewedStudentUserId == ownerUserId) return (false, _L["Err.CannotReviewSelf"]);
            var stay = await _uow.Stays.GetByIdWithDetailsAsync(stayId);
            if (stay == null) return (false, _L["Err.StayNotFound"]);
            // Owner must own the property of this stay
            if (stay.Room.Property.OwnerId != ownerUserId) return (false, _L["Err.NotPropertyOwner"]);
            if (stay.Status != StayStatus.Completed) return (false, _L["Err.ReviewAfterStayEnd"]);
            if (stay.StudentProfile.UserId != reviewedStudentUserId) return (false, _L["Err.OnlyLivedTogether"]);
            if (await _uow.Reviews.UserReviewExistsAsync(stayId, ownerUserId, reviewedStudentUserId))
                return (false, _L["Err.AlreadyReviewedRoommate"]);
            if (rating < 1 || rating > 5) return (false, _L["Err.InvalidRating"]);
            if (string.IsNullOrWhiteSpace(comment)) return (false, _L["Msg.CommentRequired"]);

            var review = new UserReview
            {
                StayId = stayId,
                ReviewerId = ownerUserId,
                ReviewedUserId = reviewedStudentUserId,
                Rating = rating,
                Comment = comment.Trim(),
                Status = ReviewStatus.Pending
            };
            await _uow.Reviews.AddUserReviewAsync(review);
            try { await _uow.SaveChangesAsync(); }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                return (false, _L["Err.AlreadyReviewedRoommate"]);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                return (false, _L["Err.ConcurrencyConflict"]);
            }
            return (true, string.Empty);
        }

        public async Task<IReadOnlyList<PropertyReview>> GetPendingAsync()
            => await _uow.Reviews.GetPendingAsync();

        public async Task<(bool Success, string Error)> ModerateAsync(int reviewId, bool approve)
        {
            var review = await _uow.Reviews.GetByIdAsync(reviewId);
            if (review == null)
            {
                return (false, _L["Err.ReviewNotFound"]);
            }

            review.Status = approve ? ReviewStatus.Approved : ReviewStatus.Removed;
            await _uow.SaveChangesAsync();

            await _notifications.CreateAsync(
                review.ReviewerId,
                approve ? _L["Notif.ReviewLive"] : _L["Notif.ReviewRemoved"],
                approve
                    ? _L["Notif.ReviewLiveBody"]
                    : _L["Notif.ReviewRemovedBody"],
                "/Student/Reviews");

            await _auditLog.LogAsync(
                approve ? "Review.Approve" : "Review.Remove",
                "Review",
                reviewId.ToString(),
                review.Title ?? review.Comment);

            return (true, string.Empty);
        }
    }
}
