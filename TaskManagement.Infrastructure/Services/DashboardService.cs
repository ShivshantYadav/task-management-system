using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;

        public DashboardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(int requestingUserId, string requestingUserRole)
        {
            IQueryable<Domain.Entities.TaskItem> query = _db.TaskItems
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Team);

            if (requestingUserRole == nameof(UserRole.User))
                query = query.Where(t => t.AssignedToId == requestingUserId);
            else if (requestingUserRole == nameof(UserRole.Manager))
                query = query.Where(t => t.Team.ManagerId == requestingUserId);

            var tasks = await query.ToListAsync();
            var now = DateTime.UtcNow;

            var statusByUser = tasks
                .GroupBy(t => new { t.AssignedToId, t.AssignedTo.Name })
                .Select(g => new UserTaskStatusSummaryDto
                {
                    UserId = g.Key.AssignedToId,
                    UserName = g.Key.Name,
                    ToDoCount = g.Count(t => t.Status == TaskItemStatus.ToDo),
                    InProgressCount = g.Count(t => t.Status == TaskItemStatus.InProgress),
                    DoneCount = g.Count(t => t.Status == TaskItemStatus.Done)
                })
                .OrderBy(x => x.UserName)
                .ToList();

            return new DashboardSummaryDto
            {
                ToDoCount = tasks.Count(t => t.Status == TaskItemStatus.ToDo),
                InProgressCount = tasks.Count(t => t.Status == TaskItemStatus.InProgress),
                DoneCount = tasks.Count(t => t.Status == TaskItemStatus.Done),
                OverdueCount = tasks.Count(t => t.Status != TaskItemStatus.Done && t.DueDate < now),
                StatusByUser = statusByUser,
                UpcomingTasks = tasks
                    .Where(t => t.Status != TaskItemStatus.Done)
                    .OrderBy(t => t.DueDate)
                    .Take(5)
                    .Select(t => new TaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Priority = t.Priority.ToString(),
                        Status = t.Status.ToString(),
                        DueDate = t.DueDate,
                        CreatedById = t.CreatedById,
                        CreatedByName = t.CreatedBy.Name,
                        AssignedToId = t.AssignedToId,
                        AssignedToName = t.AssignedTo.Name,
                        TeamId = t.TeamId,
                        TeamName = t.Team.Name,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    }).ToList()
            };
        }
    }
}
