using System.ComponentModel.DataAnnotations;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int TaskItemId { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;

        public TaskItem TaskItem { get; set; } = null!;
    }
}