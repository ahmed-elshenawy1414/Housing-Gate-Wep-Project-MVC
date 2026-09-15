using StudentHousing.Models;
using StudentHousing.ViewModels;

namespace StudentHousing.Mappings
{
    /// <summary>
    /// Manual projection helpers. Small and explicit on purpose so the code stays
    /// easy to follow (no AutoMapper magic).
    /// </summary>
    public static class PropertyMapper
    {
        public static PropertyCardViewModel ToCard(Property property)
        {
            var approvedReviews = property.Reviews?.Where(r => r.Status == ReviewStatus.Approved).ToList()
                                  ?? new List<PropertyReview>();

            var availableRooms = property.Rooms?.Where(r => r.IsAvailable).ToList()
                                 ?? new List<Room>();

            return new PropertyCardViewModel
            {
                Id = property.Id,
                Title = property.Title,
                City = property.City,
                State = property.State,
                PropertyType = property.PropertyType,
                MinRent = availableRooms.Count == 0 ? 0 : availableRooms.Min(r => r.RentPerMonth),
                Bedrooms = property.Bedrooms,
                IsFurnished = property.IsFurnished,
                IsFeatured = property.IsFeatured,
                AvailableFrom = property.AvailableFrom,
                PrimaryImageUrl = property.Images?.FirstOrDefault(i => i.IsPrimary)?.FilePath
                                  ?? property.Images?.FirstOrDefault()?.FilePath,
                OwnerName = property.Owner?.User?.FullName ?? "Property owner",
                OwnerIsVerified = property.Owner?.VerificationStatus == VerificationStatus.Verified,
                AverageRating = approvedReviews.Count == 0 ? 0 : Math.Round(approvedReviews.Average(r => r.Rating), 1),
                ReviewsCount = approvedReviews.Count
            };
        }
    }
}
