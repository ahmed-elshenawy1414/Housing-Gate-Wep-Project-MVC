using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Models
{
    public class SiteSetting
    {
        [Key, StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Value { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? ValueAr { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
