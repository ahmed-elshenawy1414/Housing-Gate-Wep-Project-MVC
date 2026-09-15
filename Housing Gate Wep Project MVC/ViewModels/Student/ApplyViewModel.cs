using System.ComponentModel.DataAnnotations;

namespace StudentHousing.ViewModels.Student
{
    public class ApplyViewModel
    {
        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string? Message { get; set; }
    }
}
