using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(int userId, int taskItemId, NotificationType type, string message);
        Task<List<NotificationDto>> GetForUserAsync(int userId);
        Task MarkReadAsync(int notificationId, int userId);
    }
}
