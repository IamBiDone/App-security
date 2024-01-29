using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class PasswordResetModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _authDbContext;
        [BindProperty]
        public PasswordReset PModel { get; set; }

        public PasswordResetModel(UserManager<ApplicationUser> userManager, AuthDbContext authDbContext)
        {
            _userManager = userManager;
            _authDbContext = authDbContext;
        }

        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var userEmail = HttpContext.Session.GetString("Email");
                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user != null)
                {
                    if (DateTime.UtcNow < user.PasswordChangeLimit)
                    {
                        ModelState.AddModelError("PModel.Password", "Password change is still on cooldown");
                        return Page();
                    }
                
                    try
                    {
                        var result = await _userManager.ResetPasswordAsync(user, PModel.ResetToken, PModel.Password);

                        if (result.Succeeded)
                        {
                            await LogActivityAsync(user.Id, "User Password resetted");
                            // Set the cooldown period before calling ResetPasswordAsync
                            user.PasswordChangeMin = DateTime.UtcNow.AddMinutes(6);
                            user.PasswordChangeLimit = DateTime.UtcNow.AddMinutes(1);

                            return RedirectToPage("/Login");
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                await LogActivityAsync(user.Id, "User Failed to reset Password");
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Password reset failed: {ex.Message}");
                        ModelState.AddModelError("", "Password reset failed. Please try again.");
                    }
                }
                else
                {
                    return NotFound();
                }
            }

            return Page();
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