using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Ном ҳатмӣ аст")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Насаб ҳатмӣ аст")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Номи корбар ҳатмӣ аст")]
    [MinLength(3, ErrorMessage = "Номи корбар камаш 3 аломат")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email ҳатмӣ аст")]
    [EmailAddress(ErrorMessage = "Формати email нодуруст")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Парол ҳатмӣ аст")]
    [MinLength(6, ErrorMessage = "Парол камаш 6 аломат")]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "User";
}
