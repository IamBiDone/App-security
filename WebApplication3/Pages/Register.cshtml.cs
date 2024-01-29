using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using WebApplication3.Model;

using WebApplication3.ViewModels;

    namespace WebApplication3.Pages
    {
        public class RegisterModel : PageModel
        {

		private readonly IHttpContextAccessor contxt;

		private UserManager<ApplicationUser> userManager { get; }
            private SignInManager<ApplicationUser> signInManager { get; }

		[BindProperty]
            public Register RModel { get; set; }

            public RegisterModel(UserManager<ApplicationUser> userManager,

            SignInManager<ApplicationUser> signInManager)
            {
                this.userManager = userManager;
                this.signInManager = signInManager;

		}

		private bool IsJpeg(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return false;
			}

			byte[] fileSignature = new byte[2];

			using (var reader = new BinaryReader(file.OpenReadStream()))
			{
				reader.Read(fileSignature, 0, fileSignature.Length);
			}

			return fileSignature.SequenceEqual(new byte[] { 0xFF, 0xD8 }); 
		}
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Customize the regular expression based on your requirements
            var phoneRegex = new Regex(@"^[0-9]{8}$");

            return phoneRegex.IsMatch(phoneNumber);
        }
        private bool IsValidCredit(string creditCard)
        {
            // Customize the regular expression based on your requirements
            var creditRegex = new Regex("(?<!\\d)\\d{16}(?!\\d)|(?<!\\d[ _-])(?<!\\d)\\d{4}(?:[_ -]\\d{4}){3}(?![_ -]?\\d)");

            return creditRegex.IsMatch(creditCard);
        }
        public async Task<IActionResult> OnPostAsync()
            {
			if (!IsJpeg(RModel.Photo))
			{
				ModelState.AddModelError("RModel.Photo", "Please select a valid JPEG photo.");
				return Page();
			}
            if (!IsValidEmail(RModel.Email))
            {
                ModelState.AddModelError("RModel.Email", "Invalid email format.");
				return Page();
			}
            if (!IsValidPhoneNumber(RModel.MobileNumber.ToString()))
            {
                ModelState.AddModelError("RModel.MobileNumber", "Invalid phone number format.");
				return Page();
			}
            if (!IsValidCredit(RModel.CreditCard.ToString()))
            {
                ModelState.AddModelError("RModel.CreditCard", "Invalid card number format.");
				return Page();
			}
            if (RModel.Photo == null || RModel.Photo.Length == 0)
			{
				ModelState.AddModelError("RModel.Photo", "Please select a valid photo.");
				return Page();
			}

			if (ModelState.IsValid)
                {

                    var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                    var protector = dataProtectionProvider.CreateProtector("mySecretKey");
                    var UploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploadfolder");

                    var FilePath = Path.Combine(UploadFolder, RModel.Photo.FileName);

                using (var fileStream = new FileStream(FilePath, FileMode.Create))
                {
                    await RModel.Photo.CopyToAsync(fileStream);
                }

                    var user = new ApplicationUser()
                    {
                        UserName = HtmlEncoder.Default.Encode(RModel.FullName),
                        Email = RModel.Email,
                        FullName = HtmlEncoder.Default.Encode(RModel.FullName),
                        CreditCard = protector.Protect(RModel.CreditCard),
                        Photo = FilePath,
                        Gender = HtmlEncoder.Default.Encode(RModel.Gender),
                        DeliveryAddress = HtmlEncoder.Default.Encode(RModel.DeliveryAddress),
                        AboutMe = HtmlEncoder.Default.Encode(RModel.AboutMe),
                        PhoneNumber = HtmlEncoder.Default.Encode(RModel.MobileNumber.ToString()),
                        TwoFactorCode= HtmlEncoder.Default.Encode("0"),
                        PasswordChangeMin = DateTime.UtcNow.AddMinutes(10), 
                        PasswordChangeLimit = DateTime.UtcNow 
                    };

                    var result = await userManager.CreateAsync(user, RModel.Password);
                    if (result.Succeeded)
                    {
                        await signInManager.SignInAsync(user, false);
                        return RedirectToPage("Login");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return Page();
            }







        }
    }
