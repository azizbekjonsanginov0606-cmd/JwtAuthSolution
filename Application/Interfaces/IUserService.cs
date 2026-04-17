using Application.DTOs;
using Application.Pagination;
using Application.Results;

namespace Application.Interfaces;

public interface IUserService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task<Result> ForgotPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(string email, ResetPasswordDto dto);
    Task<Result<PagedResult<UserDto>>> GetAllAsync(PaginationParams pagination);
    Task<Result<UserDto>> GetByIdAsync(string id);
    Task<Result<UserDto>> UpdateAsync(string id, UpdateUserDto dto);
    Task<Result> DeleteAsync(string id);
    Task<Result> AssignRoleAsync(AssignRoleDto dto);
    Task<Result> RemoveRoleAsync(AssignRoleDto dto);
}
