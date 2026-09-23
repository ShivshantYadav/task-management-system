namespace TaskManagement.Application.DTOs
{
    public class UserTaskStatusSummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int ToDoCount { get; set; }
        public int InProgressCount { get; set; }
        public int DoneCount { get; set; }
    }

    public class DashboardSummaryDto
    {
        public int ToDoCount { get; set; }
        public int InProgressCount { get; set; }
        public int DoneCount { get; set; }
        public int OverdueCount { get; set; }
        public List<UserTaskStatusSummaryDto> StatusByUser { get; set; } = new();
        public List<TaskDto> UpcomingTasks { get; set; } = new();
    }
}
