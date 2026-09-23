using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class TeamService : ITeamService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IEmailService _emailService;

        public TeamService(AppDbContext db, IPasswordHasher hasher, IEmailService emailService)
        {
            _db = db;
            _hasher = hasher;
            _emailService = emailService;
        }

        public async Task<TeamDto> CreateTeamAsync(CreateTeamDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new BadRequestException("Team name is required.");

            var manager = await _db.Users.FindAsync(dto.ManagerId)
                ?? throw new NotFoundException("Manager not found.");

            if (manager.Role != UserRole.Manager && manager.Role != UserRole.Admin)
                throw new BadRequestException("Assigned manager must have the Manager or Admin role.");

            var team = new Team
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                ManagerId = dto.ManagerId
            };

            _db.Teams.Add(team);
            await _db.SaveChangesAsync();

            return await GetTeamByIdAsync(team.Id, dto.ManagerId, nameof(UserRole.Admin));
        }

        public async Task<TeamDto> AddMemberAsync(int teamId, int requestingUserId, string requestingUserRole, AddTeamMemberDto dto)
        {
            var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == teamId)
                ?? throw new NotFoundException("Team not found.");

            if (requestingUserRole != nameof(UserRole.Admin) && team.ManagerId != requestingUserId)
                throw new ForbiddenException("Only the team's manager or an admin can add members.");

            var user = await _db.Users.FindAsync(dto.UserId)
                ?? throw new NotFoundException("User not found.");

            if (requestingUserRole == nameof(UserRole.Manager) && user.Role != UserRole.User)
                throw new BadRequestException("Managers can add User-role members to their team only.");

            var alreadyMember = await _db.TeamMembers.AnyAsync(tm => tm.TeamId == teamId && tm.UserId == dto.UserId);
            if (alreadyMember)
                throw new ConflictException("User is already a member of this team.");

            _db.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = dto.UserId });
            await _db.SaveChangesAsync();

            return await GetTeamByIdAsync(teamId, requestingUserId, requestingUserRole);
        }

        public async Task<TeamMemberCreationResultDto> CreateMemberAsync(int teamId, int requestingUserId, string requestingUserRole, CreateTeamMemberRequestDto dto)
        {
            var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == teamId)
                ?? throw new NotFoundException("Team not found.");

            if (requestingUserRole != nameof(UserRole.Admin) && team.ManagerId != requestingUserId)
                throw new ForbiddenException("Only the team's manager or an admin can add members.");

            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
                throw new BadRequestException("Name and email are required.");

            var emailNormalized = dto.Email.Trim().ToLowerInvariant();
            var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == emailNormalized);
            if (exists)
                throw new ConflictException("A user with this email already exists.");

            var temporaryPassword = TemporaryPasswordGenerator.Generate();

            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = emailNormalized,
                PasswordHash = _hasher.Hash(temporaryPassword),
                Role = UserRole.User,
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = user.Id });
            await _db.SaveChangesAsync();

            var emailSent = await _emailService.SendAccountCreatedEmailAsync(
                user.Email, user.Name, temporaryPassword, nameof(UserRole.User), team.Name);

            var teamDto = await GetTeamByIdAsync(teamId, requestingUserId, requestingUserRole);

            return new TeamMemberCreationResultDto
            {
                Team = teamDto,
                User = new UserDto { Id = user.Id, Name = user.Name, Email = user.Email, Role = user.Role.ToString() },
                EmailSent = emailSent,
                TemporaryPassword = emailSent ? null : temporaryPassword
            };
        }

        public async Task<List<TeamDto>> GetTeamsAsync(int requestingUserId, string requestingUserRole)
        {
            IQueryable<Team> query = _db.Teams
                .Include(t => t.Manager)
                .Include(t => t.TeamMembers).ThenInclude(tm => tm.User);

            if (requestingUserRole == nameof(UserRole.Manager))
            {
                query = query.Where(t => t.ManagerId == requestingUserId);
            }
            else if (requestingUserRole == nameof(UserRole.User))
            {
                query = query.Where(t => t.TeamMembers.Any(tm => tm.UserId == requestingUserId));
            }
            // Admin sees all teams.

            var teams = await query.ToListAsync();
            return teams.Select(MapToDto).ToList();
        }

        public async Task<TeamDto> GetTeamByIdAsync(int teamId, int requestingUserId, string requestingUserRole)
        {
            var team = await _db.Teams
                .Include(t => t.Manager)
                .Include(t => t.TeamMembers).ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(t => t.Id == teamId)
                ?? throw new NotFoundException("Team not found.");

            var canView = requestingUserRole == nameof(UserRole.Admin)
                || (requestingUserRole == nameof(UserRole.Manager) && team.ManagerId == requestingUserId)
                || (requestingUserRole == nameof(UserRole.User) && team.TeamMembers.Any(tm => tm.UserId == requestingUserId));

            if (!canView)
                throw new ForbiddenException("You do not have access to this team.");

            return MapToDto(team);
        }

        private static TeamDto MapToDto(Team team) => new()
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            ManagerId = team.ManagerId,
            ManagerName = team.Manager.Name,
            Members = team.TeamMembers.Select(tm => new TeamMemberDto
            {
                UserId = tm.UserId,
                Name = tm.User.Name,
                Email = tm.User.Email
            }).ToList()
        };
    }
}
