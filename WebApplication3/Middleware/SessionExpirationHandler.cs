// SessionExpirationHandler.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

public class SessionExpirationHandler
{
    private readonly RequestDelegate _next;

    public SessionExpirationHandler(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var authToken = context.Session.GetString("AuthToken");
        var cookieAuthToken = context.Request.Cookies["AuthToken"];

        if (authToken != cookieAuthToken)
        {
            // Tokens don't match, log the user out from this device
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Redirect or take appropriate action
            context.Response.Redirect("/login"); // Adjust the login page URL
            return;
        }

        // Continue processing the request
        await _next(context);
    }
}