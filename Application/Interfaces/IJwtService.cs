using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(AppUser user);
    string? ValidateToken(string token);
}
