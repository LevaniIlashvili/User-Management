using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using UserManagement.Models;

namespace UserManagement.Middleware;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    public UserStatusMiddleware(RequestDelegate next, ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _next = next;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
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
                var tempData = _tempDataDictionaryFactory.GetTempData(context);

                tempData.Clear();

                tempData.Save();

                await signInManager.SignOutAsync();
                context.Response.Redirect("/Account/Login");
                return;
            }
        }
        await _next(context);
    }
}
