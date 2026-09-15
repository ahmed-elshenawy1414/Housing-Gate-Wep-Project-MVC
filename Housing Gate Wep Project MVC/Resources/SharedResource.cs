using System.Resources;

namespace StudentHousing.Resources
{
    /// <summary>
    /// Marker type for the shared resource file (SharedResource.resx).
    /// Common strings used across all areas: navigation, buttons, enums, statuses.
    /// The ResourceManager property lets the generated validation-message accessors
    /// (see SharedResource.Validation.cs, produced by tools\gen-resources.ps1) and
    /// the DataAnnotations infrastructure read localized text from the same embedded
    /// resx used by IStringLocalizer.
    /// </summary>
    public partial class SharedResource
    {
        public static ResourceManager ResourceManager =>
            new ResourceManager(typeof(SharedResource));
    }
}
