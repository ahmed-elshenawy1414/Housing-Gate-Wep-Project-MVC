using System.Globalization;

namespace StudentHousing.Helpers
{
    /// <summary>
    /// Chooses the Arabic display name stored in the database when the current
    /// UI culture is Arabic, falling back to the English name otherwise.
    /// </summary>
    public static class LocalizedData
    {
        public static string Name(string? nameAr, string name)
            => CultureInfo.CurrentUICulture.Name.StartsWith("ar", System.StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(nameAr)
                ? nameAr
                : name;
    }
}
