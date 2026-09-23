using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces
{
    public interface ITeamService
    {
        Task<TeamDto> CreateTeamAsync(CreateTeamDto dto);
        Task<TeamDto> AddMemberAsync(int teamId, int requestingUserId, string requestingUserRole, AddTeamMemberDto dto);

        /// <summary>Manager/Admin: create a brand-new User and add them to this team, emailing credentials.</summary>
        Task<TeamMemberCreationResultDto> CreateMemberAsync(int teamId, int requestingUserId, string requestingUserRole, CreateTeamMemberRequestDto dto);
        Task<List<TeamDto>> GetTeamsAsync(int requestingUserId, string requestingUserRole);
        Task<TeamDto> GetTeamByIdAsync(int teamId, int requestingUserId, string requestingUserRole);
    }
}
