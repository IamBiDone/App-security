using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebApplication3.ViewModels
{
    public class Register
    {
		[Required]
		[DataType(DataType.EmailAddress)]
		[Remote("IsEmailUnique", "Account")]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Text)]
		public string Gender {  get; set; }
		[Required]
		[DataType(DataType.PhoneNumber)]
		public int MobileNumber { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string DeliveryAddress { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string AboutMe {  get; set; }
		[Required]
		[DataType(DataType.Upload)]
		public IFormFile Photo { get; set; }

		[Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string FullName { get; set; }

		[Required]
		[DataType(DataType.CreditCard)]
		public string CreditCard { get; set; }

		[Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
        public string ConfirmPassword { get; set; }

    }
}
