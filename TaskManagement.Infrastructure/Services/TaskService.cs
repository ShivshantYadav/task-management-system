using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notifications;

        public TaskService(AppDbContext db, INotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, int requestingUserId, string requestingUserRole)
        {
            if (requestingUserRole == nameof(UserRole.User))
                throw new ForbiddenException("Users cannot create tasks.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new BadRequestException("Title is required.");

            var team = await _db.Teams.Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == dto.TeamId)
                ?? throw new NotFoundException("Team not found.");

            if (requestingUserRole == nameof(UserRole.Manager) && team.ManagerId != requestingUserId)
                throw new ForbiddenException("Managers can only create tasks for their own team.");

            var assignee = await _db.Users.FindAsync(dto.AssignedToId)
                ?? throw new NotFoundException("Assigned user not found.");

            var isMember = team.ManagerId == dto.AssignedToId ||
                           team.TeamMembers.Any(tm => tm.UserId == dto.AssignedToId);
            if (!isMember)
                throw new BadRequestException("Assigned user must belong to the selected team.");

            if (!Enum.TryParse<TaskPriority>(dto.Priority, true, out var priority))
                throw new BadRequestException("Invalid priority. Use Low, Medium or High.");

            var task = new Domain.Entities.TaskItem
            {
                Title = dto.Title.Trim(),
                Description = dto.Description,
                Priority = priority,
                Status = TaskItemStatus.ToDo,
                DueDate = dto.DueDate,
                CreatedById = requestingUserId,
                AssignedToId = dto.AssignedToId,
                TeamId = dto.TeamId
            };

            _db.TaskItems.Add(task);
            await _db.SaveChangesAsync();

            await _notifications.CreateAsync(
                dto.AssignedToId,
                task.Id,
                NotificationType.TaskAssigned,
                $"You have been assigned a new task: \"{task.Title}\".");

            return await GetByIdAsync(task.Id, requestingUserId, requestingUserRole);
        }

        public async Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int requestingUserId, string requestingUserRole)
        {
            var task = await _db.TaskItems.Include(t => t.Team).ThenInclude(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == taskId)
                ?? throw new NotFoundException("Task not found.");

            if (requestingUserRole == nameof(UserRole.User))
                throw new ForbiddenException("Users cannot edit task details.");

            if (requestingUserRole == nameof(UserRole.Manager) && task.Team.ManagerId != requestingUserId)
                throw new ForbiddenException("Managers can only edit tasks belonging to their own team.");

            if (!Enum.TryParse<TaskPriority>(dto.Priority, true, out var priority))
                throw new BadRequestException("Invalid priority. Use Low, Medium or High.");

            var isMember = task.Team.ManagerId == dto.AssignedToId ||
                           task.Team.TeamMembers.Any(tm => tm.UserId == dto.AssignedToId);
            if (!isMember)
                throw new BadRequestException("Assigned user must belong to the task's team.");

            var reassigned = task.AssignedToId != dto.AssignedToId;

            task.Title = dto.Title.Trim();
            task.Description = dto.Description;
            task.Priority = priority;
            task.DueDate = dto.DueDate;
            task.AssignedToId = dto.AssignedToId;
            task.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            if (reassigned)
            {
                await _notifications.CreateAsync(
                    dto.AssignedToId,
                    task.Id,
                    NotificationType.TaskAssigned,
                    $"You have been assigned to task: \"{task.Title}\".");
            }

            return await GetByIdAsync(task.Id, requestingUserId, requestingUserRole);
        }

        public async Task<TaskDto> UpdateStatusAsync(int taskId, UpdateTaskStatusDto dto, int requestingUserId, string requestingUserRole)
        {
            var task = await _db.TaskItems.Include(t => t.Team)
                .FirstOrDefaultAsync(t => t.Id == taskId)
                ?? throw new NotFoundException("Task not found.");

            var isOwnerOrManager = requestingUserRole == nameof(UserRole.Admin) ||
                                    (requestingUserRole == nameof(UserRole.Manager) && task.Team.ManagerId == requestingUserId) ||
                                    task.AssignedToId == requestingUserId;

            if (!isOwnerOrManager)
                throw new ForbiddenException("You can only update the status of tasks assigned to you.");

            if (!Enum.TryParse<TaskItemStatus>(dto.Status, true, out var status))
                throw new BadRequestException("Invalid status. Use ToDo, InProgress or Done.");

            if (task.Status == status)
                return await GetByIdAsync(task.Id, requestingUserId, requestingUserRole);

            task.Status = status;
            task.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var notificationRecipientId = task.CreatedById != requestingUserId
                ? task.CreatedById
                : task.AssignedToId;

            await _notifications.CreateAsync(
                notificationRecipientId,
                task.Id,
                NotificationType.TaskStatusUpdated,
                $"Task \"{task.Title}\" status changed to {status}.");

            return await GetByIdAsync(task.Id, requestingUserId, requestingUserRole);
        }

        public async Task<TaskDto> GetByIdAsync(int taskId, int requestingUserId, string requestingUserRole)
        {
            var task = await _db.TaskItems
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Team)
                .FirstOrDefaultAsync(t => t.Id == taskId)
                ?? throw new NotFoundException("Task not found.");

            EnsureCanView(task, requestingUserId, requestingUserRole);

            return MapToDto(task);
        }

        public async Task<List<TaskDto>> GetTasksAsync(TaskFilterDto filter, int requestingUserId, string requestingUserRole)
        {
            IQueryable<Domain.Entities.TaskItem> query = _db.TaskItems
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Team).ThenInclude(t => t.TeamMembers);

            if (requestingUserRole == nameof(UserRole.User))
            {
                query = query.Where(t => t.AssignedToId == requestingUserId);
            }
            else if (requestingUserRole == nameof(UserRole.Manager))
            {
                query = query.Where(t => t.Team.ManagerId == requestingUserId);
            }
            // Admin sees all tasks.

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                Enum.TryParse<TaskItemStatus>(filter.Status, true, out var status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(filter.Priority) &&
                Enum.TryParse<TaskPriority>(filter.Priority, true, out var priority))
            {
                query = query.Where(t => t.Priority == priority);
            }

            if (filter.DueBefore.HasValue)
                query = query.Where(t => t.DueDate <= filter.DueBefore.Value);

            if (filter.DueAfter.HasValue)
                query = query.Where(t => t.DueDate >= filter.DueAfter.Value);

            var tasks = await query.OrderBy(t => t.DueDate).ToListAsync();
            return tasks.Select(MapToDto).ToList();
        }

        private static void EnsureCanView(Domain.Entities.TaskItem task, int requestingUserId, string requestingUserRole)
        {
            if (requestingUserRole == nameof(UserRole.Admin))
                return;

            if (requestingUserRole == nameof(UserRole.Manager) && task.Team.ManagerId == requestingUserId)
                return;

            if (task.AssignedToId == requestingUserId || task.CreatedById == requestingUserId)
                return;

            throw new ForbiddenException("You do not have access to this task.");
        }

        private static TaskDto MapToDto(Domain.Entities.TaskItem task) => new()
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority.ToString(),
            Status = task.Status.ToString(),
            DueDate = task.DueDate,
            CreatedById = task.CreatedById,
            CreatedByName = task.CreatedBy.Name,
            AssignedToId = task.AssignedToId,
            AssignedToName = task.AssignedTo.Name,
            TeamId = task.TeamId,
            TeamName = task.Team.Name,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
