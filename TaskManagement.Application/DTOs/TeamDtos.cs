using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs
{
    public class CreateTeamDto
    {
        [Required(ErrorMessage = "Team name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Team name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500,
            ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a valid manager.")]
        public int ManagerId { get; set; }
    }

    public class AddTeamMemberDto
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a valid user.")]
        public int UserId { get; set; }
    }

    public class CreateTeamMemberRequestDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
    }

    public class TeamMemberCreationResultDto
    {
        public TeamDto Team { get; set; } = null!;
        public UserDto User { get; set; } = null!;
        public bool EmailSent { get; set; }
        public string? TemporaryPassword { get; set; }
    }

    public class TeamMemberDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public List<TeamMemberDto> Members { get; set; } = new();
    }
}