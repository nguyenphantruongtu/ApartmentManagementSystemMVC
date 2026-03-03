using System.ComponentModel.DataAnnotations;

namespace FinalProject_ApartmentManagementSystem.ViewModels
{
    public class UserProfileViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, ErrorMessage = "Username must not exceed 100 characters.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(255, ErrorMessage = "Full name must not exceed 255 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters.")]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL.")]
        [StringLength(500, ErrorMessage = "Avatar URL must not exceed 500 characters.")]
        [Display(Name = "Avatar URL")]
        public string? AvatarUrl { get; set; }
    }
}
