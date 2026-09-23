using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notifications;

        public CommentService(AppDbContext db, INotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public async Task<CommentDto> AddCommentAsync(int taskId, CreateCommentDto dto, int requestingUserId, string requestingUserRole)
        {
            if (string.IsNullOrWhiteSpace(dto.CommentText))
                throw new BadRequestException("Comment text is required.");

            var task = await _db.TaskItems.Include(t => t.Team)
                .FirstOrDefaultAsync(t => t.Id == taskId)
                ?? throw new NotFoundException("Task not found.");

            EnsureCanAccess(task, requestingUserId, requestingUserRole);

            var comment = new Comment
            {
                TaskItemId = taskId,
                UserId = requestingUserId,
                CommentText = dto.CommentText.Trim()
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            var notifyUserId = task.AssignedToId == requestingUserId ? task.CreatedById : task.AssignedToId;
            if (notifyUserId != requestingUserId)
            {
                await _notifications.CreateAsync(
                    notifyUserId,
                    taskId,
                    NotificationType.TaskStatusUpdated,
                    $"New comment on task \"{task.Title}\".");
            }

            var user = await _db.Users.FindAsync(requestingUserId);

            return new CommentDto
            {
                Id = comment.Id,
                TaskItemId = comment.TaskItemId,
                UserId = comment.UserId,
                UserName = user?.Name ?? string.Empty,
                CommentText = comment.CommentText,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<List<CommentDto>> GetCommentsAsync(int taskId, int requestingUserId, string requestingUserRole)
        {
            var task = await _db.TaskItems.Include(t => t.Team)
                .FirstOrDefaultAsync(t => t.Id == taskId)
                ?? throw new NotFoundException("Task not found.");

            EnsureCanAccess(task, requestingUserId, requestingUserRole);

            var comments = await _db.Comments
                .Include(c => c.User)
                .Where(c => c.TaskItemId == taskId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => new CommentDto
            {
                Id = c.Id,
                TaskItemId = c.TaskItemId,
                UserId = c.UserId,
                UserName = c.User.Name,
                CommentText = c.CommentText,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        private static void EnsureCanAccess(Domain.Entities.TaskItem task, int requestingUserId, string requestingUserRole)
        {
            if (requestingUserRole == nameof(UserRole.Admin))
                return;

            if (requestingUserRole == nameof(UserRole.Manager) && task.Team.ManagerId == requestingUserId)
                return;

            if (task.AssignedToId == requestingUserId || task.CreatedById == requestingUserId)
                return;

            throw new ForbiddenException("You do not have access to this task's comments.");
        }
    }
}
