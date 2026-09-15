using System.ComponentModel.DataAnnotations;
using StudentHousing.Models;

namespace StudentHousing.ViewModels.Student
{
    public class ComplaintFormViewModel
    {
        [Required, StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public ComplaintType ComplaintType { get; set; } = ComplaintType.Other;

        [Required, StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Reported user (optional)")]
        public string? TargetUserId { get; set; }

        [Display(Name = "Reported property (optional)")]
        public int? TargetPropertyId { get; set; }

        public List<SelectListItemWrapper> TargetUsers { get; set; } = new List<SelectListItemWrapper>();
        public List<SelectListItemWrapper> TargetProperties { get; set; } = new List<SelectListItemWrapper>();
    }

    /// <summary>Small wrapper so views don't depend on MVC's SelectListItem directly.</summary>
    public class SelectListItemWrapper
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
