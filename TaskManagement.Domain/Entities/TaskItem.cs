using System.ComponentModel.DataAnnotations;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public TaskPriority Priority { get; set; }

        [Required]
        public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;

        [Required]
        public DateTime DueDate { get; set; }

        [Range(1, int.MaxValue)]
        public int CreatedById { get; set; }

        [Range(1, int.MaxValue)]
        public int AssignedToId { get; set; }

        [Range(1, int.MaxValue)]
        public int TeamId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public User CreatedBy { get; set; } = null!;

        public User AssignedTo { get; set; } = null!;

        public Team Team { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}