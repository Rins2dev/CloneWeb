using System.ComponentModel.DataAnnotations;

namespace ViewModel
{
    public class RegisterModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string ReturnUrl { get; set; }
    }
}
