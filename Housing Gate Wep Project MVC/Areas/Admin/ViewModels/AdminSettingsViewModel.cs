using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Areas.Admin.ViewModels
{
    public class AdminSettingsViewModel
    {
        [StringLength(200)]
        public string FacebookUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string InstagramUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string TikTokUrl { get; set; } = string.Empty;
    }
}