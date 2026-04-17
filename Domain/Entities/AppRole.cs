using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class AppRole : IdentityRole
{
    public string? Description { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
}
