namespace TaskManagement.Application.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
