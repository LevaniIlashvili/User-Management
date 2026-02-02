using Microsoft.AspNetCore.Identity;
using UserManagement.Models;

namespace UserManagement.Middleware;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user == null || user.IsBlocked)
            {
                await signInManager.SignOutAsync();
                context.Response.Redirect("/Account/Login");
                return;
            }
        }
        await _next(context);
    }
}
