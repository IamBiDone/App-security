using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using WebApplication3.Model;
using Microsoft.Extensions.Logging;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class AccountRecoveryModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _authDbContext;
        private readonly ILogger<AccountRecoveryModel> _logger;
        [BindProperty]
        public EmailRequest EModel { get; set; }

        public AccountRecoveryModel(SignInManager<ApplicationUser> signInManager, ILogger<AccountRecoveryModel> logger, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, AuthDbContext authDbContext)
        {
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _authDbContext = authDbContext;
            _logger = logger;
        }

        public void OnGet()
        {
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
        private async Task SendVerificationEmail(string email, string verificationCode)
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
                Subject = "Password Reset Verification Code",
                Body = $"Your verification code for password reset is: {verificationCode}",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }
        public async Task<IActionResult> OnPost()
        {


            var user = await _userManager.FindByEmailAsync(EModel.Email);
            if (!IsValidEmail(EModel.Email))
            {
                ModelState.AddModelError("EModel.Email", "Invalid email format.");
                return Page();
            }
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User with the provided email does not exist.");
                return Page();
            }
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var verificationCode = resetToken;
            user.TwoFactorCode = verificationCode;
            await _userManager.UpdateAsync(user);
            HttpContext.Session.SetString("Email", EModel.Email);
            await LogActivityAsync(user.Id, "User attempting Password Change.");
            await SendVerificationEmail(user.Email, verificationCode);

            return RedirectToPage("/PasswordReset");
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
