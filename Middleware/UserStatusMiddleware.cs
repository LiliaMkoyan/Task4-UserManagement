using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Task4.UserManagement.Data;
using Task4.UserManagement.Models;

namespace Task4.UserManagement.Middleware;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
        {
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;

            var user = await (
                    from u in dbContext.Users
                    where u.Email == email
                    select u)
                .FirstOrDefaultAsync();

            if (user == null || user.Status == AccountStatus.Blocked)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/Account/Login");
                return;
            }
        }

        await _next(context);
    }
}