using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IEmailService _emailService;

    public UserService(AppDbContext db, IPasswordHasher hasher, IEmailService emailService)
    {
        _db = db;
        _hasher = hasher;
        _emailService = emailService;
    }

    public async Task<List<UserDto>> GetUsersAsync(int requestingUserId, string requestingUserRole, string? role = null)
    {
        if (requestingUserRole != nameof(UserRole.Admin) && requestingUserRole != nameof(UserRole.Manager))
            throw new ForbiddenException("Only Admins and Managers can view the user directory.");

        var query = _db.Users.AsNoTracking().Where(u => u.IsActive);

        if (requestingUserRole == nameof(UserRole.Manager))
            query = query.Where(u => u.Role == UserRole.User);
        else if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var parsedRole))
            query = query.Where(u => u.Role == parsedRole);

        return await query.OrderBy(u => u.Name)
            .Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email, Role = u.Role.ToString() })
            .ToListAsync();
    }

    public async Task<UserCreationResultDto> CreateUserAsync(CreateUserRequestDto dto, int requestingUserId, string requestingUserRole)
    {
        // Only an Admin can create accounts directly from the Users page. Managers
        // create employees through TeamService.CreateMemberAsync (scoped to their own team).
        if (requestingUserRole != nameof(UserRole.Admin))
            throw new ForbiddenException("Only Admins can create user accounts directly.");

        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            throw new BadRequestException("Name and email are required.");

        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role) || role == UserRole.Admin)
            throw new BadRequestException("Role must be Manager or User. Admin accounts cannot be created from this form.");

        var emailNormalized = dto.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == emailNormalized);
        if (exists)
            throw new ConflictException("A user with this email already exists.");

        Team? team = null;
        if (dto.TeamId.HasValue)
        {
            team = await _db.Teams.FindAsync(dto.TeamId.Value)
                ?? throw new NotFoundException("Team not found.");

            if (role != UserRole.User)
                throw new BadRequestException("Only User-role accounts can be added directly to a team here.");
        }

        var temporaryPassword = TemporaryPasswordGenerator.Generate();

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = emailNormalized,
            PasswordHash = _hasher.Hash(temporaryPassword),
            Role = role,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        if (team != null)
        {
            _db.TeamMembers.Add(new TeamMember { TeamId = team.Id, UserId = user.Id });
            await _db.SaveChangesAsync();
        }

        var emailSent = await _emailService.SendAccountCreatedEmailAsync(
            user.Email, user.Name, temporaryPassword, user.Role.ToString(), team?.Name);

        return new UserCreationResultDto
        {
            User = new UserDto { Id = user.Id, Name = user.Name, Email = user.Email, Role = user.Role.ToString() },
            EmailSent = emailSent,
            TemporaryPassword = emailSent ? null : temporaryPassword
        };
    }
}
