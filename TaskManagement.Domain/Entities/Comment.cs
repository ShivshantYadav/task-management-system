using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int TaskItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string CommentText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public TaskItem TaskItem { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}