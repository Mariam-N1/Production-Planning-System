namespace BackendProject.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // never the password itself - only the hash
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // "User" or "Admin". Admins are the ones who can approve production runs.
    public string Role { get; set; } = Roles.User;

    // ---- password reset ----
    // the code is hashed too, for the same reason the password is
    public string ResetCodeHash { get; set; } = string.Empty;
    public DateTime? ResetExpiresAt { get; set; }
    public int ResetAttempts { get; set; }

    // ---- account lockout ----
    public int FailedLogins { get; set; }
    public DateTime? LockedUntil { get; set; }
}

public static class Roles
{
    public const string User  = "User";
    public const string Admin = "Admin";
}
