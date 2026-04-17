using Application.DTOs;
using Application.Interfaces;
using Application.Pagination;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = RoleConstants.Admin)]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        try
        {
            var result = await _userService.GetAllAsync(pagination);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои контроллер хангоми гирифтани руйхати корбарон");
            return StatusCode(500, new { message = "Хатои дохилии сервер." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var result = await _userService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои контроллер хангоми ёфтани корбар {Id}", id);
            return StatusCode(500, new { message = "Хатои дохилии сервер." });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои контроллер хангоми навсозии корбар {Id}", id);
            return StatusCode(500, new { message = "Хатои дохилии сервер." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _userService.DeleteAsync(id);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Error });

            return Ok(new { message = "Корбар гайрифаъол шуд." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои контроллер хангоми несткунии корбар {Id}", id);
            return StatusCode(500, new { message = "Хатои дохилии сервер." });
        }
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.AssignRoleAsync(dto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Error });

            return Ok(new { message = $"Роли '{dto.Role}' таъин шуд." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои контроллер хангоми таъини рол");
            return StatusCode(500, new { message = "Хатои дохилии сервер." });
        }
    }

    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RemoveRoleAsync(dto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.Error });

            return Ok(new { message = $"Роли '{dto.Role}' гирифта шуд." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Хатои контроллер хангоми гирифтани рол");
            return StatusCode(500, new { message = "Хатои дохилии сервер." });
        }
    }
}
