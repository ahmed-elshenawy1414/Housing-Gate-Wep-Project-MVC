using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    /// <summary>
    /// A predefined university that owners/students can select from, instead of
    /// typing an arbitrary name.
    /// </summary>
    public class University
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string? NameAr { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
