using Microsoft.AspNetCore.Identity;

namespace UserManagement.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
    public DateTimeOffset LastLoginTime { get; set; }
    public DateTimeOffset RegistrationTime { get; set; }
    public bool IsBlocked { get; set; }
}
