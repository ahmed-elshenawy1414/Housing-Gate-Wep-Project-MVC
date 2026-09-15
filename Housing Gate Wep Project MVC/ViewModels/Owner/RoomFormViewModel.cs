using System.ComponentModel.DataAnnotations;
using StudentHousing.Models;

namespace StudentHousing.ViewModels.Owner
{
    public class RoomFormViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public RoomType RoomType { get; set; } = RoomType.Single;

        [Range(0, int.MaxValue), Display(Name = "Rent per month")]
        public int RentPerMonth { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Bathroom type")]
        public BathroomType BathroomType { get; set; } = BathroomType.Shared;

        [Range(0, 20), Display(Name = "Number of beds")]
        public int NumberOfBeds { get; set; } = 1;

        [Range(0, 20), Display(Name = "Available beds")]
        public int AvailableBeds { get; set; } = 1;

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;
    }
}
