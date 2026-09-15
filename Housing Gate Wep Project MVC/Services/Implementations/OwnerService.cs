using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Owner;

namespace StudentHousing.Services.Implementations
{
    public class OwnerService : IOwnerService
    {
        private readonly IUnitOfWork _uow;

        public OwnerService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<OwnerDashboardViewModel> GetDashboardAsync(string ownerId)
        {
            var profile = await _uow.OwnerProfiles.FirstOrDefaultAsync(o => o.UserId == ownerId);
            var properties = await _uow.Properties.GetByOwnerAsync(ownerId);
            var applications = await _uow.Applications.GetByOwnerAsync(ownerId);
            var stays = await _uow.Stays.GetByOwnerAsync(ownerId);
            var complaints = await _uow.Complaints.GetByPropertyOwnerAsync(ownerId);
            var reviews = await _uow.Reviews.GetByPropertyOwnerAsync(ownerId);

            return new OwnerDashboardViewModel
            {
                Profile = profile ?? new OwnerProfile { UserId = ownerId },
                TotalProperties = properties.Count,
                ApprovedProperties = properties.Count(p => p.ApprovalStatus == ApprovalStatus.Approved),
                PendingApplications = applications.Count(a => a.Status == ApplicationStatus.Pending),
                ActiveStays = stays.Count(s => s.Status == StayStatus.Active),
                OpenComplaints = complaints.Count(c => c.Status == ComplaintStatus.Open || c.Status == ComplaintStatus.UnderReview),
                PendingReviews = reviews.Count(r => r.Status == ReviewStatus.Pending),
                Properties = properties.Take(5).ToList(),
                RecentApplications = applications.Take(5).ToList(),
                RecentComplaints = complaints.Take(5).ToList(),
                RecentReviews = reviews.Where(r => r.Status == ReviewStatus.Approved).Take(5).ToList()
            };
        }
    }
}
