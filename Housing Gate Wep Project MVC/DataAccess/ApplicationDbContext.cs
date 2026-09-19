using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentHousing.Models;

namespace StudentHousing.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<OwnerProfile> OwnerProfiles => Set<OwnerProfile>();
        public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
        public DbSet<RoommatePreference> RoommatePreferences => Set<RoommatePreference>();
        public DbSet<Property> Properties => Set<Property>();
        public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Bed> Beds => Set<Bed>();
        public DbSet<Amenity> Amenities => Set<Amenity>();
        public DbSet<University> Universities => Set<University>();
        public DbSet<PropertyApplication> PropertyApplications => Set<PropertyApplication>();
        public DbSet<Stay> Stays => Set<Stay>();
        public DbSet<PropertyReview> PropertyReviews => Set<PropertyReview>();
        public DbSet<PropertyReviewImage> PropertyReviewImages => Set<PropertyReviewImage>();
        public DbSet<UserReview> UserReviews => Set<UserReview>();
        public DbSet<HousingRequest> HousingRequests => Set<HousingRequest>();
        public DbSet<HousingRequestOffer> HousingRequestOffers => Set<HousingRequestOffer>();
        public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
        public DbSet<Complaint> Complaints => Set<Complaint>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
