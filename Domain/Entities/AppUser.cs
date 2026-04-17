using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class AppUser : IdentityUser
{
    public string  FirstName  { get; set; } = string.Empty;
    public string  LastName   { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool    IsActive   { get; set; } = true;
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetExpiry { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}
