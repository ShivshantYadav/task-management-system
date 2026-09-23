using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs
{
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(1000, MinimumLength = 1,
            ErrorMessage = "Comment must be between 1 and 1000 characters.")]
        public string CommentText { get; set; } = string.Empty;
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}