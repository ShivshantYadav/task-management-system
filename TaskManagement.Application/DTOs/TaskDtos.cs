using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Task title is required.")]
        [StringLength(200, MinimumLength = 3,
            ErrorMessage = "Task title must be between 3 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000,
            ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        [RegularExpression("^(Low|Medium|High)$",
            ErrorMessage = "Priority must be Low, Medium, or High.")]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a valid user.")]
        public int AssignedToId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a valid team.")]
        public int TeamId { get; set; }
    }

    public class UpdateTaskDto
    {
        [Required(ErrorMessage = "Task title is required.")]
        [StringLength(200, MinimumLength = 3,
            ErrorMessage = "Task title must be between 3 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000,
            ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        [RegularExpression("^(Low|Medium|High)$",
            ErrorMessage = "Priority must be Low, Medium, or High.")]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a valid user.")]
        public int AssignedToId { get; set; }
    }

    public class UpdateTaskStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(To Do|In Progress|Done)$",
            ErrorMessage = "Status must be To Do, In Progress, or Done.")]
        public string Status { get; set; } = string.Empty;
    }

    public class TaskFilterDto
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? DueBefore { get; set; }
        public DateTime? DueAfter { get; set; }
    }

    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int CreatedById { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int AssignedToId { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}