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
                        var pastPasswords = _authDbContext.PasswordHistories
                            .Where(ph => ph.UserId == user.Id)
                            .OrderByDescending(ph => ph.CreatedAt)
                            .Take(2) 
                            .Select(ph => ph.HashedPassword)
                            .ToList();

                        var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, PModel.Password);


                        if (pastPasswords.Any(hash => _userManager.PasswordHasher.VerifyHashedPassword(user, hash, PModel.Password) != PasswordVerificationResult.Failed))
                        {
                            ModelState.AddModelError("PModel.Password", "The new password cannot be one of the past two passwords.");
                            return Page();
                        }

                        var result = await _userManager.ResetPasswordAsync(user, PModel.ResetToken, PModel.Password);

                        if (result.Succeeded)
                        {
                            var newPasswordHistory = new PasswordHistory
                            {
                                UserId = user.Id,
                                HashedPassword = _userManager.PasswordHasher.HashPassword(user, PModel.Password),
                                CreatedAt = DateTime.UtcNow
                            };

                            _authDbContext.PasswordHistories.Add(newPasswordHistory);
                            await _authDbContext.SaveChangesAsync();

                            await LogActivityAsync(user.Id, "User Password resetted");
                            user.PasswordChangeMin = DateTime.UtcNow.AddMinutes(6);
                            user.PasswordChangeLimit = DateTime.UtcNow.AddMinutes(1);
                            await _userManager.UpdateAsync(user);
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