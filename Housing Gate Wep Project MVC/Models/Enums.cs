namespace StudentHousing.Models
{
    public enum Gender
    {
        Male,
        Female,
        Other,
        PreferNotToSay
    }

    public enum TenantGender
    {
        Any = 0,
        Male = 1,
        Female = 2
    }

    public enum PropertyType
    {
        Apartment,
        SharedHouse,
        Studio,
        SingleRoom
    }

    /// <summary>
    /// State of a listing through its lifecycle.
    /// Pending = waiting for admin review; NeedsChanges = admin asked the owner for edits;
    /// Draft = saved but not submitted; Suspended = temporarily hidden by admin;
    /// Archived = permanently hidden / retired.
    /// </summary>
    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected,
        NeedsChanges,
        Draft,
        Suspended,
        Archived
    }

    /// <summary>State of a student / owner identity check.</summary>
    public enum VerificationStatus
    {
        Unverified,
        Pending,
        Verified,
        Rejected,
        NeedsChanges
    }

    public enum BathroomType
    {
        Private,
        Shared,
        Attached
    }

    /// <summary>Where a photo belongs on a property / room.</summary>
    public enum PropertyImageCategory
    {
        Exterior,
        Entrance,
        LivingRoom,
        Kitchen,
        Bathroom,
        Bedroom,
        Other
    }

    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }

    public enum StayStatus
    {
        Active,
        Completed,
        Cancelled
    }

    public enum ComplaintType
    {
        Harassment,
        PropertyIssue,
        Misrepresentation,
        Scam,
        Other
    }

    public enum ComplaintStatus
    {
        Open,
        UnderReview,
        Resolved,
        Dismissed
    }

    public enum RoomType
    {
        Single,
        Shared,
        Master
    }

    public enum SleepSchedule
    {
        EarlyBird,
        NightOwl,
        Flexible
    }

    public enum NoiseLevel
    {
        Quiet,
        Moderate,
        Loud
    }

    public enum Cleanliness
    {
        Tidy,
        Casual,
        Messy
    }

    public enum ReviewStatus
    {
        Pending,
        Approved,
        Removed
    }
}
