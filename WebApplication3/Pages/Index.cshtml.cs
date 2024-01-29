using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using WebApplication3.Migrations;
using WebApplication3.Model;

namespace WebApplication3.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AuthDbContext _authDbContext;

        public IndexModel(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        public IActionResult OnGet()
        {
            LogActivity("User viewed the homepage.");

            return Page();
        }
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/login");
        }
        private void LogActivity(string action)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                var auditLog = new Audit
                {
                    UserId = userId,
                    Action = action,
                    Timestamp = DateTime.Now,
                    Details = "Additional information about the activity."
                };

                _authDbContext.AuditLogs.Add(auditLog);
                _authDbContext.SaveChanges();
            }
        }
    }
}