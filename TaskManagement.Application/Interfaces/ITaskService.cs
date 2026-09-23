using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, int requestingUserId, string requestingUserRole);
        Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int requestingUserId, string requestingUserRole);
        Task<TaskDto> UpdateStatusAsync(int taskId, UpdateTaskStatusDto dto, int requestingUserId, string requestingUserRole);
        Task<TaskDto> GetByIdAsync(int taskId, int requestingUserId, string requestingUserRole);
        Task<List<TaskDto>> GetTasksAsync(TaskFilterDto filter, int requestingUserId, string requestingUserRole);
    }
}
