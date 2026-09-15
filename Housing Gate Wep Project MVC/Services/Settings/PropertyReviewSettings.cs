namespace StudentHousing.Settings
{
    /// <summary>
    /// Central rule for which property edits push an approved listing back into
    /// admin review. Read from the "PropertyReviewSettings" configuration section.
    /// </summary>
    public class PropertyReviewSettings
    {
        public bool RequireReviewOnLocationChange { get; set; } = true;
        public bool RequireReviewOnBedroomCountChange { get; set; } = true;
        public bool RequireReviewOnBathroomCountChange { get; set; } = false;
        public bool RequireReviewOnPropertyTypeChange { get; set; } = true;
        public bool RequireReviewOnFurnishingChange { get; set; } = true;
        public bool RequireReviewOnAmenityChange { get; set; } = true;
    }
}
