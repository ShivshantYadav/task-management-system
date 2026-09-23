using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Security;
using TaskManagement.Infrastructure.Services;
using Xunit;

namespace TaskManagement.Tests
{
    public class AuthServiceTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static AuthService CreateService(AppDbContext db)
        {
            var jwtGenerator = new FakeJwtTokenGenerator();
            return new AuthService(db, new PasswordHasher(), jwtGenerator);
        }

        [Fact]
        public async Task Register_CreatesUser_AndReturnsToken()
        {
            using var db = CreateContext();
            var service = CreateService(db);

            var result = await service.RegisterAsync(new RegisterRequestDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            });

            Assert.NotNull(result.Token);
            Assert.Equal("test@example.com", result.User.Email);
            Assert.Equal("User", result.User.Role);
            Assert.Single(db.Users);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsConflict()
        {
            using var db = CreateContext();
            var service = CreateService(db);

            await service.RegisterAsync(new RegisterRequestDto
            {
                Name = "First",
                Email = "dup@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            });

            await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(new RegisterRequestDto
            {
                Name = "Second",
                Email = "dup@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            }));
        }

        [Fact]
        public async Task Login_WrongPassword_ThrowsBadRequest()
        {
            using var db = CreateContext();
            var service = CreateService(db);

            await service.RegisterAsync(new RegisterRequestDto
            {
                Name = "Test User",
                Email = "test2@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            });

            await Assert.ThrowsAsync<BadRequestException>(() => service.LoginAsync(new LoginRequestDto
            {
                Email = "test2@example.com",
                Password = "WrongPassword"
            }));
        }
    }
}
