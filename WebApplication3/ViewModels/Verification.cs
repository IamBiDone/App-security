using System.ComponentModel.DataAnnotations;

namespace WebApplication3.ViewModels
{
    public class Verification
    {
        [Required]
        [DataType(DataType.Text)]
        public string OTP { get; set; }
    }
}
