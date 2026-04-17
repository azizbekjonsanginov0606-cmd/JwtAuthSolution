using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<JwtService> _logger;

    public JwtService(
        IConfiguration config,
        UserManager<AppUser> userManager,
        ILogger<JwtService> logger)
    {
        _config = config;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<string> GenerateTokenAsync(AppUser user)
    {
        try
        {
            _logger.LogDebug("JWT token сохтан барои корбар {UserName}", user.UserName);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name,           user.UserName!),
                new(ClaimTypes.Email,          user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("fullName", user.FullName)
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var hours = double.Parse(_config["Jwt:ExpiresInHours"] ?? "2");
            var expires = DateTime.UtcNow.AddHours(hours);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            _logger.LogInformation(
                "JWT token барои {UserName} бо ролхои [{Roles}] сохта шуд",
                user.UserName, string.Join(", ", roles));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хато хангоми сохтани JWT барои {UserName}", user.UserName);
            throw;
        }
    }

    public string? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;
            var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Token санчиш нокомёб: {Error}", ex.Message);
            return null;
        }
    }
}
