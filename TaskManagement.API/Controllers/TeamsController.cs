using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Extensions;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/teams")]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TeamDto>>> GetTeams()
        {
            var teams = await _teamService.GetTeamsAsync(User.GetUserId(), User.GetRole());
            return Ok(teams);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TeamDto>> GetTeam(int id)
        {
            var team = await _teamService.GetTeamByIdAsync(id, User.GetUserId(), User.GetRole());
            return Ok(team);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto dto)
        {
            var team = await _teamService.CreateTeamAsync(dto);
            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
        }

        [HttpPost("{id:int}/members")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<TeamDto>> AddMember(int id, AddTeamMemberDto dto)
        {
            var team = await _teamService.AddMemberAsync(id, User.GetUserId(), User.GetRole(), dto);
            return Ok(team);
        }

        // Manager/Admin: create a brand-new employee account and add them to this team in
        // one step (instead of picking an existing user). Credentials are emailed to them.
        [HttpPost("{id:int}/members/create")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<TeamMemberCreationResultDto>> CreateMember(int id, CreateTeamMemberRequestDto dto)
        {
            var result = await _teamService.CreateMemberAsync(id, User.GetUserId(), User.GetRole(), dto);
            return Ok(result);
        }
    }
}
