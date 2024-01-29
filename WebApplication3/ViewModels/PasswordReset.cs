using System.ComponentModel.DataAnnotations;

namespace WebApplication3.ViewModels
{
    public class PasswordReset
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
        public string ConfirmPassword { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string ResetToken { get; set; }
    }
}
