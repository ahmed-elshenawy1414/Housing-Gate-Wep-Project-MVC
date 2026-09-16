using System.ComponentModel.DataAnnotations;

namespace StudentHousing.Areas.Admin.ViewModels
{
    public class AdminSettingsViewModel
    {
        [StringLength(200), Display(Name = "Facebook URL")]
        public string FacebookUrl { get; set; } = string.Empty;

        [StringLength(200), Display(Name = "Instagram URL")]
        public string InstagramUrl { get; set; } = string.Empty;

        [StringLength(200), Display(Name = "TikTok URL")]
        public string TikTokUrl { get; set; } = string.Empty;

        // Homepage hero
        [StringLength(500), Display(Name = "Homepage Hero Title")]
        public string HomeHeroTitle { get; set; } = string.Empty;

        [StringLength(1000), Display(Name = "Homepage Hero Subtitle")]
        public string HomeHeroSub { get; set; } = string.Empty;

        // About Us
        [StringLength(500), Display(Name = "About Lead")]
        public string AboutLead { get; set; } = string.Empty;

        [StringLength(2000), Display(Name = "About Body")]
        public string AboutBody { get; set; } = string.Empty;

        // How It Works
        [StringLength(500), Display(Name = "How It Works - Step 1")]
        public string HowItWorksStep1 { get; set; } = string.Empty;

        [StringLength(500), Display(Name = "How It Works - Step 2")]
        public string HowItWorksStep2 { get; set; } = string.Empty;

        [StringLength(500), Display(Name = "How It Works - Step 3")]
        public string HowItWorksStep3 { get; set; } = string.Empty;
    }
}