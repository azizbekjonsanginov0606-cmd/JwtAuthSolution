using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Номи корбар ҳатмӣ аст")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Парол ҳатмӣ аст")]
    public string Password { get; set; } = string.Empty;
}
