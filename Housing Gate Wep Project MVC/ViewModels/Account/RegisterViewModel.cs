using System.ComponentModel.DataAnnotations;
using StudentHousing.Resources;

namespace StudentHousing.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required]
        public string AccountType { get; set; } = "Student";

        [Required, Display(Name = "First name"), StringLength(60)]
        public string FirstName { get; set; } = string.Empty;

        [Required, Display(Name = "Last name"), StringLength(60)]
        public string LastName { get; set; } = string.Empty;

        [Required, Phone, StringLength(20), Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password), Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessageResourceName = "PasswordMismatch_ValidationError", ErrorMessageResourceType = typeof(SharedResource))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "TermsRequired_ValidationError", ErrorMessageResourceType = typeof(SharedResource))]
        public bool AgreeToTerms { get; set; }
    }
}
