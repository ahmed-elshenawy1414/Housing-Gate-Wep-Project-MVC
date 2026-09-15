using Microsoft.Extensions.Localization;
using StudentHousing.Models;

namespace StudentHousing.Helpers
{
    /// <summary>
    /// Friendly labels for the platform's enums, used in views and match breakdowns.
    /// When a localizer is supplied the label is read from resources (Enums.<Enum>.<Value>)
    /// so it follows the current culture; otherwise an English fallback is returned.
    /// </summary>
    public static class EnumDisplay
    {
        private static string Localized<TEnum>(TEnum value, IStringLocalizer? localizer) where TEnum : struct, Enum
        {
            if (localizer == null)
            {
                return string.Empty;
            }

            var entry = localizer[$"Enums.{typeof(TEnum).Name}.{value}"];
            return entry.ResourceNotFound ? string.Empty : entry.Value;
        }

        /// <summary>
        /// Generic overload for any enum (ApplicationStatus, StayStatus, ApprovalStatus,
        /// VerificationStatus, ...). Reads Enums.<Enum>.<Value> from resources, falling
        /// back to the English member name.
        /// </summary>
        public static string Get<TEnum>(TEnum value, IStringLocalizer? localizer = null) where TEnum : struct, Enum
        {
            var localized = Localized(value, localizer);
            return localized.Length > 0 ? localized : value.ToString();
        }

        public static string Get(Gender value, IStringLocalizer? localizer = null)
        {
            var localized = Localized(value, localizer);
            if (localized.Length > 0) { return localized; }
            return value switch
            {
                Gender.Male => "Male",
                Gender.Female => "Female",
                Gender.Other => "Other",
                _ => "Prefer not to say"
            };
        }

        public static string Get(PropertyType value, IStringLocalizer? localizer = null)
        {
            var localized = Localized(value, localizer);
            if (localized.Length > 0) { return localized; }
            return value switch
            {
                PropertyType.Apartment => "Apartment",
                PropertyType.SharedHouse => "Shared house",
                PropertyType.Studio => "Studio",
                _ => "Single room"
            };
        }

        public static string Get(SleepSchedule value, IStringLocalizer? localizer = null)
        {
            var localized = Localized(value, localizer);
            if (localized.Length > 0) { return localized; }
            return value switch
            {
                SleepSchedule.EarlyBird => "Early bird",
                SleepSchedule.NightOwl => "Night owl",
                _ => "Flexible"
            };
        }

        public static string Get(NoiseLevel value, IStringLocalizer? localizer = null)
        {
            var localized = Localized(value, localizer);
            if (localized.Length > 0) { return localized; }
            return value switch
            {
                NoiseLevel.Quiet => "Quiet",
                NoiseLevel.Loud => "Lively",
                _ => "Moderate"
            };
        }

        public static string Get(Cleanliness value, IStringLocalizer? localizer = null)
        {
            var localized = Localized(value, localizer);
            if (localized.Length > 0) { return localized; }
            return value switch
            {
                Cleanliness.Tidy => "Tidy",
                Cleanliness.Messy => "Messy",
                _ => "Casual"
            };
        }

        public static string Get(ComplaintType value, IStringLocalizer? localizer = null)
        {
            var localized = Localized(value, localizer);
            if (localized.Length > 0) { return localized; }
            return value switch
            {
                ComplaintType.Harassment => "Harassment",
                ComplaintType.PropertyIssue => "Property issue",
                ComplaintType.Misrepresentation => "Misrepresentation",
                ComplaintType.Scam => "Scam",
                _ => "Other"
            };
        }

        public static string Get(ComplaintStatus value, IStringLocalizer? localizer = null)
        {
            var localized = Localized(value, localizer);
            if (localized.Length > 0) { return localized; }
            return value switch
            {
                ComplaintStatus.Open => "Open",
                ComplaintStatus.UnderReview => "Under review",
                ComplaintStatus.Resolved => "Resolved",
                _ => "Dismissed"
            };
        }
    }
}
