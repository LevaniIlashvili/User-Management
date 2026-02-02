using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using UserManagement.Models;
using UserManagement.Services;

namespace UserManagement.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _emailSender;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManger,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManger;
        _emailSender = emailSender;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel registerModel)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser()
            {
                UserName = registerModel.Email,
                Email = registerModel.Email,
                Name = registerModel.FirstName + " " + registerModel.LastName,
                RegistrationTime = DateTimeOffset.UtcNow,
                LastLoginTime = DateTimeOffset.UtcNow,
                IsBlocked = false
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerModel.Password);

                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var link = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                    await _emailSender.SendEmailAsync(user.Email, "Confirm your account",
                        $"Please confirm your account by <a href='{link}'>clicking here</a>.");

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["Info"] = "Email confirmation sent to your email";

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                if (IsUniqueConstraintViolation(ex))
                {
                    ModelState.AddModelError("Email", "This e-mail is already taken");
                }
                else
                {
                    throw;
                }
            }
        }

        return View(registerModel);
    }

    private static bool IsUniqueConstraintViolation(Exception ex)
    {
        var inner = ex.InnerException;

        while (inner != null)
        {
            if (inner is PostgresException postgresEx && postgresEx.SqlState == "23505")
            {
                return true;
            }

            inner = inner.InnerException;
        }

        return false;
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (userId == null || token == null) return RedirectToAction("Index", "Home");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
            TempData["Success"] = "Email confirmed successfully!";
        else
            TempData["Error"] = "Error confirming email.";

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginModel)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user != null && !user.IsBlocked)
            {
                user.LastLoginTime = DateTimeOffset.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            var result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, false, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(loginModel);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}
