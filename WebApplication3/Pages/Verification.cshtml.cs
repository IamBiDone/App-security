using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using System.Net;
using System;
using WebApplication3.Model;
using WebApplication3.ViewModels;
using Microsoft.AspNetCore.DataProtection;

namespace WebApplication3.Pages
{
    public class VerificationModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _authDbContext;
        private readonly Random _random = new Random();
		private readonly IDataProtectionProvider dataProtectionProvider;
		[BindProperty]
        public Verification VModel { get; set; }
        public VerificationModel(SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, AuthDbContext authDbContext)
        {
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _authDbContext = authDbContext;
		}

        public async Task OnGet()
        {
        }



        public async Task<IActionResult> OnPost()
        {

            var userEmail = HttpContext.Session.GetString("Email");

            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user != null)
            {

                var storedOTP = user.TwoFactorCode;


                var enteredOTP = VModel.OTP;

                if (enteredOTP == storedOTP)
                {
					var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
					var protector = dataProtectionProvider.CreateProtector("mySecretKey");
					var decryptedCreditCard = protector.Unprotect(user.CreditCard);

					HttpContext.Session.SetString("Authorization", "Authorized");
					HttpContext.Session.SetString("UserID", user.Id);
					HttpContext.Session.SetString("FullName", user.FullName);
					HttpContext.Session.SetString("PhoneNumber", user.PhoneNumber.ToString());
					HttpContext.Session.SetString("Gender", user.Gender);
					HttpContext.Session.SetString("CreditCard", decryptedCreditCard);
					HttpContext.Session.SetString("DeliveryAddress", user.DeliveryAddress);
					HttpContext.Session.SetString("AboutMe", user.AboutMe);
					await _signInManager.SignInAsync(user, isPersistent: false);

                    await LogActivityAsync(user.Id, "User successfully verified using OTP.");

                    return RedirectToPage("/Index");
                }
                else
                {

                    ModelState.AddModelError(string.Empty, "Invalid verification code.");
                    return Page();
                }
            }
            else
            {

                return NotFound();
            }
        }

        private async Task LogActivityAsync(string userId, string action)
        {
            var auditLog = new Audit
            {
                UserId = userId,
                Action = action,
                Timestamp = DateTime.Now,
                Details = "Additional information about the activity."
            };

            _authDbContext.AuditLogs.Add(auditLog);
            await _authDbContext.SaveChangesAsync();
        }
    }
}
