using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenGenerator _jwt;

        public AuthService(AppDbContext db, IPasswordHasher hasher, IJwtTokenGenerator jwt)
        {
            _db = db;
            _hasher = hasher;
            _jwt = jwt;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                throw new BadRequestException("Name, email and password are required.");
            }

            if (request.Password.Length < 6)
            {
                throw new BadRequestException("Password must be at least 6 characters long.");
            }

            if (request.Password != request.ConfirmPassword)
            {
                throw new BadRequestException("Password and confirm password do not match.");
            }

            var emailNormalized = request.Email.Trim().ToLowerInvariant();

            var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == emailNormalized);
            if (exists)
                throw new ConflictException("A user with this email already exists.");

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = emailNormalized,
                PasswordHash = _hasher.Hash(request.Password),
                Role = UserRole.User,
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return BuildResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var emailNormalized = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalized);

            if (user == null || !user.IsActive || !_hasher.Verify(request.Password, user.PasswordHash))
                throw new BadRequestException("Invalid email or password.");

            return BuildResponse(user);
        }

        private AuthResponseDto BuildResponse(User user)
        {
            var (token, expires) = _jwt.GenerateToken(user);
            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expires,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };
        }
    }
}
