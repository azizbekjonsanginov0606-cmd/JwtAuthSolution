using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Application.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterAsync(dto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Error });

            return StatusCode(result.StatusCode, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои контроллер хангоми бакайдгирӣ");
            return StatusCode(500, new { message = "Хатои дохилии сервер." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.LoginAsync(dto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои контроллер хангоми воридшавӣ");
            return StatusCode(500, new { message = "Хатои дохилии сервер." });
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _userService.ChangePasswordAsync(userId, dto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Error });

            return Ok(new { message = "Парол бомуваффакият иваз шуд." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои контроллер хангоми иваз кардани парол");
            return StatusCode(500, new { message = "Хатои дохилии сервер." });
        }
    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _userService.ForgotPasswordAsync(dto.Email);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);

        var result = await _userService.ResetPasswordAsync(email!, dto);

        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _userService.GetByIdAsync(userId);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои контроллер хангоми гирифтани маълумоти корбари кунунӣ");
            return StatusCode(500, new { message = "Хатои дохилии сервер." });
        }
    }
}
