using StudentHousing.Data;
using StudentHousing.Models;
using StudentHousing.Repositories.Implementations;
using StudentHousing.Repositories.Interfaces;

namespace StudentHousing.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        private IPropertyRepository? _properties;
        private IStudentProfileRepository? _studentProfiles;
        private IApplicationRepository? _applications;
        private IStayRepository? _stays;
        private IComplaintRepository? _complaints;
        private IReviewRepository? _reviews;
        private INotificationRepository? _notifications;
        private IAuditLogRepository? _auditLogs;
        private IUserRepository? _users;
        private IRepository<OwnerProfile>? _ownerProfiles;
        private IRepository<Room>? _rooms;
        private IRepository<RoommatePreference>? _roommatePreferences;
        private IRepository<Amenity>? _amenities;
        private IRepository<Bed>? _beds;
        private IRepository<University>? _universities;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
        }

        public IPropertyRepository Properties => _properties ??= new PropertyRepository(_db);
        public IStudentProfileRepository StudentProfiles => _studentProfiles ??= new StudentProfileRepository(_db);
        public IApplicationRepository Applications => _applications ??= new ApplicationRepository(_db);
        public IStayRepository Stays => _stays ??= new StayRepository(_db);
        public IComplaintRepository Complaints => _complaints ??= new ComplaintRepository(_db);
        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_db);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_db);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_db);
        public IUserRepository Users => _users ??= new UserRepository(_db);
        public IRepository<OwnerProfile> OwnerProfiles => _ownerProfiles ??= new Repository<OwnerProfile>(_db);
        public IRepository<Room> Rooms => _rooms ??= new Repository<Room>(_db);
        public IRepository<RoommatePreference> RoommatePreferences => _roommatePreferences ??= new Repository<RoommatePreference>(_db);
        public IRepository<Amenity> Amenities => _amenities ??= new Repository<Amenity>(_db);
        public IRepository<Bed> Beds => _beds ??= new Repository<Bed>(_db);
        public IRepository<University> Universities => _universities ??= new Repository<University>(_db);

        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
