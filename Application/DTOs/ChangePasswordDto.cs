using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Пароли кунунӣ ҳатмӣ аст")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароли нав ҳатмӣ аст")]
    [MinLength(6, ErrorMessage = "Пароли нав камаш 6 аломат")]
    public string NewPassword { get; set; } = string.Empty;
}