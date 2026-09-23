using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class CreateUserRequestDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(User|Manager)$",
            ErrorMessage = "Role must be either User or Manager.")]
        public string Role { get; set; } = "User";

        public int? TeamId { get; set; }
    }

    public class UserCreationResultDto
    {
        public UserDto User { get; set; } = null!;
        public bool EmailSent { get; set; }
        public string? TemporaryPassword { get; set; }
    }
}