using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class ChangePasswordModel : PageModel
    {
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;

		[BindProperty]
		public ChangePassword CModel { get; set; }

		public ChangePasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		public void OnGet()
		{
			// Handle GET request if needed
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.GetUserAsync(User);

				if (user == null)
				{
					return NotFound("User not found.");
				}

				var changePasswordResult = await _userManager.ChangePasswordAsync(user, CModel.OldPassword, CModel.NewPassword);

				if (changePasswordResult.Succeeded)
				{
					// Sign out the user to force re-login with the new password
					await _signInManager.SignOutAsync();

					// Optionally, sign in the user again if needed
					// await _signInManager.SignInAsync(user, isPersistent: false);

					return RedirectToPage("/Login"); // Redirect to login page after successful password change
				}
				else
				{
					foreach (var error in changePasswordResult.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}

					return Page();
				}
			}

			return Page();
		}
	}
}
