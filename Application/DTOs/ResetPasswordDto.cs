using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ResetPasswordDto
{
    public string Code { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}