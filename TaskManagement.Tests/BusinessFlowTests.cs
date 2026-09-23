using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Security;
using TaskManagement.Infrastructure.Services;
using Xunit;

namespace TaskManagement.Tests
{
    // End-to-end business-flow coverage: role rules + notification triggers.
    public class BusinessFlowTests
    {
        private static AppDbContext CreateContext() => new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        [Fact]
        public async Task Register_PasswordTooShort_ThrowsBadRequest()
        {
            using var db = CreateContext();
            var service = new AuthService(db, new PasswordHasher(), new FakeJwtTokenGenerator());

            await Assert.ThrowsAsync<BadRequestException>(() => service.RegisterAsync(new RegisterRequestDto
            {
                Name = "Test",
                Email = "short@example.com",
                Password = "123",
                ConfirmPassword = "123"
            }));
        }

        [Fact]
        public async Task Login_InactiveUser_ThrowsBadRequest()
        {
            using var db = CreateContext();
            var service = new AuthService(db, new PasswordHasher(), new FakeJwtTokenGenerator());

            await service.RegisterAsync(new RegisterRequestDto
            {
                Name = "Test",
                Email = "inactive@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            });

            db.Users.Single().IsActive = false;
            await db.SaveChangesAsync();

            await Assert.ThrowsAsync<BadRequestException>(() => service.LoginAsync(new LoginRequestDto
            {
                Email = "inactive@example.com",
                Password = "Password123!"
            }));
        }

        [Fact]
        public async Task CreateTask_UserRole_ThrowsForbidden()
        {
            using var db = CreateContext();
            SeedTeam(db);
            await db.SaveChangesAsync();

            var service = new TaskService(db, new FakeNotificationService(db));

            await Assert.ThrowsAsync<ForbiddenException>(() => service.CreateTaskAsync(new CreateTaskDto
            {
                Title = "User cannot create",
                Priority = "Low",
                DueDate = DateTime.UtcNow.AddDays(1),
                TeamId = 1,
                AssignedToId = 2
            }, 2, nameof(UserRole.User)));
        }

        [Fact]
        public async Task CreateTask_ManagerOfOtherTeam_ThrowsForbidden()
        {
            using var db = CreateContext();
            SeedTeam(db);
            db.Users.Add(new User { Id = 3, Name = "Other Manager", Email = "other@test.com", Role = UserRole.Manager, PasswordHash = "x" });
            db.Teams.Add(new Team { Id = 2, Name = "Marketing", ManagerId = 3 });
            await db.SaveChangesAsync();

            var service = new TaskService(db, new FakeNotificationService(db));

            // Manager 3 tries to create a task on team 1 (managed by manager 1)
            await Assert.ThrowsAsync<ForbiddenException>(() => service.CreateTaskAsync(new CreateTaskDto
            {
                Title = "Cross-team task",
                Priority = "Medium",
                DueDate = DateTime.UtcNow.AddDays(1),
                TeamId = 1,
                AssignedToId = 2
            }, 3, nameof(UserRole.Manager)));
        }

        [Fact]
        public async Task UpdateStatus_NonAssignee_ThrowsForbidden()
        {
            using var db = CreateContext();
            SeedTeam(db);
            db.Users.Add(new User { Id = 4, Name = "Outsider", Email = "out@test.com", Role = UserRole.User, PasswordHash = "x" });
            db.TaskItems.Add(new TaskItem
            {
                Id = 1,
                Title = "Task",
                Priority = TaskPriority.Low,
                Status = TaskItemStatus.ToDo,
                DueDate = DateTime.UtcNow.AddDays(1),
                CreatedById = 1,
                AssignedToId = 2,
                TeamId = 1
            });
            await db.SaveChangesAsync();

            var service = new TaskService(db, new FakeNotificationService(db));

            await Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateStatusAsync(1, new UpdateTaskStatusDto
            {
                Status = "Done"
            }, 4, nameof(UserRole.User)));
        }

        [Fact]
        public async Task UpdateStatus_ChangesStatus_AndNotifiesCreator()
        {
            using var db = CreateContext();
            SeedTeam(db);
            db.TaskItems.Add(new TaskItem
            {
                Id = 1,
                Title = "Task",
                Priority = TaskPriority.Low,
                Status = TaskItemStatus.ToDo,
                DueDate = DateTime.UtcNow.AddDays(1),
                CreatedById = 1,
                AssignedToId = 2,
                TeamId = 1
            });
            await db.SaveChangesAsync();

            var service = new TaskService(db, new FakeNotificationService(db));

            var result = await service.UpdateStatusAsync(1, new UpdateTaskStatusDto
            {
                Status = "InProgress"
            }, 2, nameof(UserRole.User));

            Assert.Equal("InProgress", result.Status);
            var notification = Assert.Single(db.Notifications);
            Assert.Equal(NotificationType.TaskStatusUpdated, notification.Type);
            Assert.Equal(1, notification.UserId); // creator (manager) is notified
        }

        private static void SeedTeam(AppDbContext db)
        {
            db.Users.AddRange(
                new User { Id = 1, Name = "Manager", Email = "m@test.com", Role = UserRole.Manager, PasswordHash = "x" },
                new User { Id = 2, Name = "Member", Email = "u@test.com", Role = UserRole.User, PasswordHash = "x" });
            db.Teams.Add(new Team { Id = 1, Name = "Engineering", ManagerId = 1 });
            db.TeamMembers.Add(new TeamMember { TeamId = 1, UserId = 2 });
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
}
