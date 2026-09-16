using System.ComponentModel.DataAnnotations;
using StudentHousing.Models;

namespace StudentHousing.ViewModels.Student
{
    /// <summary>Submitting a review for a property after a verified stay.</summary>
    public class PropertyReviewFormViewModel
    {
        [Required]
        public int StayId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; } = string.Empty;

        // Photos: optional, visible to anyone viewing the property listing
        public List<IFormFile>? Photos { get; set; }

        // Display helpers
        public string PropertyTitle { get; set; } = string.Empty;
        public string? RoomName { get; set; }
    }
}
