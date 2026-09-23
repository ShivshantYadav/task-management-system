using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    // NOTE: Real email sending is not wired up (per assignment: "mock notifications
    // if email integration is not implemented"). Every event is persisted to the
    // Notifications table and can be polled by the frontend / listed via GET /api/notifications.
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(int userId, int taskItemId, NotificationType type, string message)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                TaskItemId = taskItemId,
                Type = type,
                Message = message
            });
            await _db.SaveChangesAsync();
        }

        public async Task<List<NotificationDto>> GetForUserAsync(int userId)
        {
            var items = await _db.Notifications
                .Include(n => n.TaskItem)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return items.Select(n => new NotificationDto
            {
                Id = n.Id,
                TaskItemId = n.TaskItemId,
                TaskTitle = n.TaskItem.Title,
                Type = n.Type.ToString(),
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        public async Task MarkReadAsync(int notificationId, int userId)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId)
                ?? throw new NotFoundException("Notification not found.");

            if (notification.UserId != userId)
                throw new ForbiddenException("You cannot modify another user's notification.");

            notification.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }
}
