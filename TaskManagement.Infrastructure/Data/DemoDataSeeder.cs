using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Infrastructure.Data;
public static class DemoDataSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher)
    {
        if (await db.Users.AnyAsync())
            return;

        var admin = new User
        {
            Name = "Demo Admin",
            Email = "admin@demo.com",
            PasswordHash = hasher.Hash("Admin@123"),
            Role = UserRole.Admin,
            IsActive = true
        };
        var manager = new User
        {
            Name = "Demo Manager",
            Email = "manager@demo.com",
            PasswordHash = hasher.Hash("Manager@123"),
            Role = UserRole.Manager,
            IsActive = true
        };
        var user = new User
        {
            Name = "Demo User",
            Email = "user@demo.com",
            PasswordHash = hasher.Hash("User@123"),
            Role = UserRole.User,
            IsActive = true
        };

        db.Users.AddRange(admin, manager, user);
        await db.SaveChangesAsync();

        var team = new Team
        {
            Name = "Product Engineering",
            Description = "Demo team for the assessment walkthrough.",
            ManagerId = manager.Id
        };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        db.TeamMembers.Add(new TeamMember { TeamId = team.Id, UserId = user.Id });

        var task = new TaskItem
        {
            Title = "Prepare project walkthrough",
            Description = "Review the task flow, comments, status updates and notifications before submission.",
            Priority = TaskPriority.High,
            Status = TaskItemStatus.ToDo,
            DueDate = DateTime.UtcNow.Date.AddDays(3),
            CreatedById = manager.Id,
            AssignedToId = user.Id,
            TeamId = team.Id
        };
        db.TaskItems.Add(task);
        await db.SaveChangesAsync();

        db.Notifications.Add(new Notification
        {
            UserId = user.Id,
            TaskItemId = task.Id,
            Type = NotificationType.TaskAssigned,
            Message = $"You have been assigned a new task: \"{task.Title}\"."
        });

        await db.SaveChangesAsync();
    }
}
