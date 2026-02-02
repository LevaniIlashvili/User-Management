using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            var users = await _dbContext.Users.OrderByDescending(u => u.LastLoginTime).ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Block(string[] selectedUserIds)
        {
            if (selectedUserIds != null && selectedUserIds.Length > 0)
            {
                var usersToBlock = await _dbContext.Users
                    .Where(u => selectedUserIds.Contains(u.Id) && !u.IsBlocked)
                    .ToListAsync();

                int count = usersToBlock.Count;
                if (count > 0)
                {
                    usersToBlock.ForEach(user => user.IsBlocked = true);
                    await _dbContext.SaveChangesAsync();
                    TempData["Success"] = $"Successfully blocked {count} user(s).";
                }
                else
                {
                    TempData["Info"] = "Selected users were already blocked.";
                }
            }
            else
            {
                TempData["Error"] = "No users selected.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string[] selectedUserIds)
        {
            if (selectedUserIds != null && selectedUserIds.Length > 0)
            {
                var usersToUnblock = await _dbContext.Users
                    .Where(u => selectedUserIds.Contains(u.Id) && u.IsBlocked)
                    .ToListAsync();

                int count = usersToUnblock.Count;
                if (count > 0)
                {
                    usersToUnblock.ForEach(user => user.IsBlocked = false);
                    await _dbContext.SaveChangesAsync();
                    TempData["Success"] = $"Successfully unblocked {count} user(s).";
                }
                else
                {
                    TempData["Info"] = "Selected users were already unblocked.";
                }
            }
            else
            {
                TempData["Error"] = "No users selected.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string[] selectedUserIds)
        {
            if (selectedUserIds != null && selectedUserIds.Length > 0)
            {
                var usersToDelete = await _dbContext.Users
                    .Where(u => selectedUserIds.Contains(u.Id))
                    .ToListAsync();

                int count = usersToDelete.Count;
                if (count > 0)
                {
                    _dbContext.RemoveRange(usersToDelete);
                    await _dbContext.SaveChangesAsync();
                    TempData["Success"] = $"Successfully deleted {count} user(s).";
                }
                else
                {
                    TempData["Error"] = "The selected users no longer exist.";
                }
            }
            else
            {
                TempData["Error"] = "No users selected.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUnverified(string[] selectedUserIds)
        {
            if (selectedUserIds != null && selectedUserIds.Length > 0)
            {
                var unverifiedUsers = await _dbContext.Users
                    .Where(u => selectedUserIds.Contains(u.Id) && !u.EmailConfirmed)
                    .ToListAsync();

                int count = unverifiedUsers.Count;

                if (count > 0)
                {
                    _dbContext.Users.RemoveRange(unverifiedUsers);
                    await _dbContext.SaveChangesAsync();
                    TempData["Success"] = $"Deleted {count} unverified account(s)";
                }
                else
                {
                    TempData["Info"] = "None of the selected users were unverified.";
                }
            }
            else
            {
                TempData["Error"] = "No users selected.";
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
