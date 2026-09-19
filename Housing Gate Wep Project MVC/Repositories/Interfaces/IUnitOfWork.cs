using StudentHousing.Models;

namespace StudentHousing.Repositories.Interfaces
{
    /// <summary>
    /// Groups all repositories so services can work against a single, consistent
    /// set of data-access objects and commit changes atomically.
    /// </summary>
    public interface IUnitOfWork
    {
        IPropertyRepository Properties { get; }
        IStudentProfileRepository StudentProfiles { get; }
        IApplicationRepository Applications { get; }
        IStayRepository Stays { get; }
        IComplaintRepository Complaints { get; }
        IReviewRepository Reviews { get; }
        INotificationRepository Notifications { get; }
        IAuditLogRepository AuditLogs { get; }
        IUserRepository Users { get; }
        IRepository<OwnerProfile> OwnerProfiles { get; }
        IRepository<Room> Rooms { get; }
        IRepository<RoommatePreference> RoommatePreferences { get; }
        IRepository<Amenity> Amenities { get; }
        IRepository<Bed> Beds { get; }
        IRepository<University> Universities { get; }
        IHousingRequestRepository HousingRequests { get; }
        IRepository<HousingRequestOffer> HousingRequestOffers { get; }

        Task<int> SaveChangesAsync();
    }
}
