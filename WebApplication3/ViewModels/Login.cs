using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace WebApplication3.ViewModels
{
	public class Login
	{
		[Required]
		[DataType(DataType.Text)]
		public string FullName { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

	}
}
