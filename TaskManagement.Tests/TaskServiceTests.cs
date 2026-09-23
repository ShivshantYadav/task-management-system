using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Services;
using Xunit;

namespace TaskManagement.Tests;

public class TaskServiceTests
{
    private static AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task CreateTask_NotifiesAssignedUser()
    {
        using var db = CreateContext();
        var manager = new User { Id = 1, Name = "Manager", Email = "m@test.com", Role = UserRole.Manager, PasswordHash = "x" };
        var user = new User { Id = 2, Name = "User", Email = "u@test.com", Role = UserRole.User, PasswordHash = "x" };
        var team = new Team { Id = 1, Name = "Engineering", ManagerId = 1 };
        db.Users.AddRange(manager, user);
        db.Teams.Add(team);
        db.TeamMembers.Add(new TeamMember { TeamId = 1, UserId = 2 });
        await db.SaveChangesAsync();

        var notifications = new FakeNotificationService(db);
        var service = new TaskService(db, notifications);

        var result = await service.CreateTaskAsync(new CreateTaskDto
        {
            Title = "Assessment task",
            Priority = "High",
            DueDate = DateTime.UtcNow.AddDays(2),
            TeamId = 1,
            AssignedToId = 2
        }, 1, nameof(UserRole.Manager));

        Assert.Equal("ToDo", result.Status);
        Assert.Equal(2, result.AssignedToId);
        Assert.Single(db.Notifications);
        Assert.Equal(NotificationType.TaskAssigned, db.Notifications.Single().Type);
    }

    [Fact]
    public async Task CreateTask_ManagerCannotAssignOutsideOwnTeam()
    {
        using var db = CreateContext();
        db.Users.AddRange(
            new User { Id = 1, Name = "Manager", Email = "m@test.com", Role = UserRole.Manager, PasswordHash = "x" },
            new User { Id = 2, Name = "Outside User", Email = "u@test.com", Role = UserRole.User, PasswordHash = "x" });
        db.Teams.Add(new Team { Id = 1, Name = "Engineering", ManagerId = 1 });
        await db.SaveChangesAsync();

        var service = new TaskService(db, new FakeNotificationService(db));

        await Assert.ThrowsAsync<TaskManagement.Application.Common.BadRequestException>(() => service.CreateTaskAsync(new CreateTaskDto
        {
            Title = "Invalid assignment",
            Priority = "Medium",
            DueDate = DateTime.UtcNow.AddDays(1),
            TeamId = 1,
            AssignedToId = 2
        }, 1, nameof(UserRole.Manager)));
    }

    private sealed class FakeNotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        public FakeNotificationService(AppDbContext db) => _db = db;

        public async Task CreateAsync(int userId, int taskItemId, NotificationType type, string message)
        {
            _db.Notifications.Add(new Notification { UserId = userId, TaskItemId = taskItemId, Type = type, Message = message });
            await _db.SaveChangesAsync();
        }

        public Task<List<NotificationDto>> GetForUserAsync(int userId) => Task.FromResult(new List<NotificationDto>());
        public Task MarkReadAsync(int notificationId, int userId) => Task.CompletedTask;
    }
}
