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



        private async Task<int> GenerateOTP()
        {
            // Generate a random 2-digit OTP
            int otp = _random.Next(10, 100);

            return otp;
        }

        private async Task SendOTPEmail(string email, int otp)
        {
            // Send the OTP to the user's email
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("t8406480@gmail.com", "ioqc qyvx nvxv nipv"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("t8406480@gmail.com"),
                Subject = "Two-Factor Authentication OTP",
                Body = $"Your OTP for Two-Factor Authentication is: {otp}",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
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

                    HttpContext.Session.SetString("Authorization", "Authorized");

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
