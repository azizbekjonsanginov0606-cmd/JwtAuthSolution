using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Ном ҳатмӣ аст")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Насаб ҳатмӣ аст")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email ҳатмӣ аст")]
    [EmailAddress(ErrorMessage = "Формати email нодуруст")]
    public string Email { get; set; } = string.Empty;
}
