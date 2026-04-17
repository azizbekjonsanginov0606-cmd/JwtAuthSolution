using Application.DTOs;
using Application.Interfaces;
using Application.Pagination;
using Application.Results;
using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IJwtService jwtService,
        IEmailService emailService,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }



    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        try
        {
            _logger.LogInformation("Бакайдгирии корбар: {UserName}", dto.UserName);

            if (await _userManager.FindByNameAsync(dto.UserName) is not null)
            {
                _logger.LogWarning("Корбар {UserName} аллакай мавчуд аст", dto.UserName);
                return Result<AuthResponseDto>.Failure("Корбар бо ин ном аллакай мавчуд аст.", 409);
            }

            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            {
                _logger.LogWarning("Email {Email} аллакай истифода мешавад", dto.Email);
                return Result<AuthResponseDto>.Failure("Ин email аллакай кайд шудааст.", 409);
            }

            var user = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Сохтани корбар {UserName} нокомёб: {Errors}",
                    dto.UserName, string.Join(" | ", errors));
                return Result<AuthResponseDto>.Failure(string.Join("; ", errors));
            }

            var role = RoleConstants.AllRoles.Contains(dto.Role) ? dto.Role : RoleConstants.User;
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new AppRole { Name = role });

            await _userManager.AddToRoleAsync(user, role);
            var roles = await _userManager.GetRolesAsync(user);

            _ = _emailService.SendWelcomeEmailAsync(user.Email!, user.UserName!);

            _logger.LogInformation(
                "Корбар {UserName} (ID:{Id}) бо роли [{Role}] кайд шуд",
                user.UserName, user.Id, role);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                UserName = user.UserName!,
                UserId = user.Id,
                Roles = roles,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            }, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои гайричашмдошт хангоми бакайдгирии {UserName}", dto.UserName);
            return Result<AuthResponseDto>.ServerError();
        }
    }


    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            _logger.LogInformation("Воридшавии корбар: {UserName}", dto.UserName);

            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                _logger.LogWarning("Воридшавии нокомёб барои {UserName}", dto.UserName);
                return Result<AuthResponseDto>.Unauthorized("Ном ё парол нодуруст аст.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Корбари гайрифаъол {UserName} кушиш кард ворид шавад", dto.UserName);
                return Result<AuthResponseDto>.Failure("Хисоби корбар гайрифаъол аст.", 403);
            }

            var token = await _jwtService.GenerateTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation(
                "Корбар {UserName} (ID:{Id}) бомуваффакият ворид шуд",
                user.UserName, user.Id);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token,
                UserName = user.UserName!,
                UserId = user.Id,
                Roles = roles,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои гайричашмдошт хангоми воридшавии {UserName}", dto.UserName);
            return Result<AuthResponseDto>.ServerError();
        }
    }


    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        try
        {
            _logger.LogInformation("Корбар {UserId} паролро иваз мекунад", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("Корбар {UserId} ёфт нашуд", userId);
                return Result.NotFound("Корбар ёфт нашуд.");
            }

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Иваз кардани пароли {UserId} нокомёб: {Errors}",
                    userId, string.Join(" | ", errors));
                return Result.Failure(string.Join("; ", errors));
            }

            _ = _emailService.SendPasswordChangedEmailAsync(user.Email!, user.UserName!);
            _logger.LogInformation("Пароли корбар {UserId} бомуваффакият иваз шуд", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои гайричашмдошт хангоми иваз кардани парол {UserId}", userId);
            return Result.ServerError();
        }
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure("User not found");

        var code = new Random().Next(100000, 999999).ToString();

        user.PasswordResetCode = code;
        user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(10);

        await _userManager.UpdateAsync(user);

        await _emailService.SendAsync(new EmailDto
        {
            To = email,
            Subject = "Password Reset Code",
            Body = $"Your code is: <b>{code}</b>"
        });

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(string email, ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure("User not found");

        if (user.PasswordResetCode != dto.Code ||
            user.PasswordResetExpiry < DateTime.UtcNow)
            return Result.Failure("Invalid or expired code");

        await _userManager.RemovePasswordAsync(user);
        await _userManager.AddPasswordAsync(user, dto.NewPassword);

        user.PasswordResetCode = null;
        user.PasswordResetExpiry = null;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }





    public async Task<Result<PagedResult<UserDto>>> GetAllAsync(PaginationParams pagination)
    {
        try
        {
            _logger.LogInformation("Руйхати корбарон: сахифа {Page}, андоза {Size}",
                pagination.PageNumber, pagination.PageSize);

            var query = _userManager.Users.Where(u => u.IsActive);
            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userDtos.Add(MapToDto(u, roles));
            }

            return Result<PagedResult<UserDto>>.Success(
                PagedResult<UserDto>.Create(userDtos, totalCount,
                    pagination.PageNumber, pagination.PageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хато хангоми гирифтани руйхати корбарон");
            return Result<PagedResult<UserDto>>.ServerError();
        }
    }


    public async Task<Result<UserDto>> GetByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Дархости корбар ID: {Id}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                _logger.LogWarning("Корбар {Id} ёфт нашуд", id);
                return Result<UserDto>.NotFound($"Корбар бо ID '{id}' ёфт нашуд.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Result<UserDto>.Success(MapToDto(user, roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хато хангоми ёфтани корбар {Id}", id);
            return Result<UserDto>.ServerError();
        }
    }



    public async Task<Result<UserDto>> UpdateAsync(string id, UpdateUserDto dto)
    {
        try
        {
            _logger.LogInformation("Навсозии корбар {Id}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                _logger.LogWarning("Навсозӣ рад шуд: корбар {Id} ёфт нашуд", id);
                return Result<UserDto>.NotFound();
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Навсозии {Id} нокомёб: {Errors}", id, string.Join(" | ", errors));
                return Result<UserDto>.Failure(string.Join("; ", errors));
            }

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("Корбар {Id} бомуваффакият навсозӣ шуд", id);
            return Result<UserDto>.Success(MapToDto(user, roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хато хангоми навсозии корбар {Id}", id);
            return Result<UserDto>.ServerError();
        }
    }



    public async Task<Result> DeleteAsync(string id)
    {
        try
        {
            _logger.LogInformation("Несткунии корбар {Id}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                _logger.LogWarning("Несткунӣ рад шуд: корбар {Id} ёфт нашуд", id);
                return Result.NotFound();
            }

            // Soft delete — корбарро гайрифаъол мекунем
            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation(
                "Корбар {UserName} (ID:{Id}) гайрифаъол шуд", user.UserName, id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хато хангоми несткунии корбар {Id}", id);
            return Result.ServerError();
        }
    }



    public async Task<Result> AssignRoleAsync(AssignRoleDto dto)
    {
        try
        {
            _logger.LogInformation(
                "Таъини роли [{Role}] ба корбар {UserId}", dto.Role, dto.UserId);

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user is null) return Result.NotFound("Корбар ёфт нашуд.");

            if (!RoleConstants.AllRoles.Contains(dto.Role))
                return Result.Failure($"Роли '{dto.Role}' вучуд надорад.");

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new AppRole { Name = dto.Role });

            if (await _userManager.IsInRoleAsync(user, dto.Role))
                return Result.Failure($"Корбар аллакай дар роли '{dto.Role}' мебошад.");

            var result = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!result.Succeeded)
                return Result.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation(
                "Роли [{Role}] ба корбар {UserId} таъин шуд", dto.Role, dto.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хато хангоми таъини рол ба корбар {UserId}", dto.UserId);
            return Result.ServerError();
        }
    }


    public async Task<Result> RemoveRoleAsync(AssignRoleDto dto)
    {
        try
        {
            _logger.LogInformation(
                "Гирифтани роли [{Role}] аз корбар {UserId}", dto.Role, dto.UserId);

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user is null) return Result.NotFound("Корбар ёфт нашуд.");

            if (!await _userManager.IsInRoleAsync(user, dto.Role))
                return Result.Failure($"Корбар дар роли '{dto.Role}' нест.");

            var result = await _userManager.RemoveFromRoleAsync(user, dto.Role);
            if (!result.Succeeded)
                return Result.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation(
                "Роли [{Role}] аз корбар {UserId} гирифта шуд", dto.Role, dto.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хато хангоми гирифтани рол аз корбар {UserId}", dto.UserId);
            return Result.ServerError();
        }
    }


    private static UserDto MapToDto(AppUser user, IList<string> roles) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        FullName = user.FullName,
        UserName = user.UserName!,
        Email = user.Email!,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        Roles = roles
    };
}

