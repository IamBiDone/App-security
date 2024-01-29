using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using WebApplication3.Model;
using WebApplication3.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;
using WebApplication3.Migrations;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace WebApplication3.Pages
{

    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _authDbContext;

        [BindProperty]
        public Login LModel { get; set; }

        public LoginModel(SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, AuthDbContext authDbContext)
        {
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _authDbContext = authDbContext;
        }

        private readonly Random _random = new Random();

        private async Task<int> GenerateOTP()
        {
            int otp = _random.Next(10, 100);


            return otp;
        }

        private async Task SendOTPEmail(string email, int otp)
        {
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


        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByNameAsync(LModel.FullName);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt.");
                return Page();
            }

            if (await IsRateLimitExceeded(user.Id))
            {
                ModelState.AddModelError(string.Empty, "Rate limit exceeded. Try again later.");
                return Page();
            }

            if ( user.PasswordChangeMin <= DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Password change required. Please reset your password.");
                return Page();
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(LModel.FullName, LModel.Password, false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var loggedInUser = await _userManager.FindByNameAsync(LModel.FullName);
                    var otp = GenerateOTP().Result;
                    SendOTPEmail(loggedInUser.Email, otp).Wait();
                    loggedInUser.TwoFactorCode = HtmlEncoder.Default.Encode(otp.ToString());
                    HttpContext.Session.SetString("Email", loggedInUser.Email);

                    await LogActivityAsync(loggedInUser.Id, "User logged in, awaiting verification.");

                    return RedirectToPage("/Verification");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Login attempt");
                    await IncrementAccessFailedCount(user.Id);

                    await LogActivityAsync(user.Id, "Failed login attempt.");

                    return Page();
                }
            }

            return Page();
        }

        private async Task<bool> IsRateLimitExceeded(string userId)
        {
            const int maxAttempts = 2;
            const int timeoutMinutes = 1;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            return user.LockoutEnabled &&
                user.LockoutEnd >= DateTimeOffset.UtcNow &&
                user.AccessFailedCount >= maxAttempts &&
                user.LockoutEnd <= DateTimeOffset.UtcNow.AddMinutes(timeoutMinutes);
        }

        private async Task IncrementAccessFailedCount(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.AccessFailedCount++;

                if (user.AccessFailedCount >= 2)
                {
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(1);
                }

                await _userManager.UpdateAsync(user);
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