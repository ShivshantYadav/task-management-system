using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Extensions;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers([FromQuery] string? role)
        => Ok(await _userService.GetUsersAsync(User.GetUserId(), User.GetRole(), role));

    // Admin-only: create a Manager or User account directly. A temporary password is
    // generated and emailed to the new account; see EmailService/Smtp config.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserCreationResultDto>> CreateUser(CreateUserRequestDto dto)
    {
        var result = await _userService.CreateUserAsync(dto, User.GetUserId(), User.GetRole());
        return CreatedAtAction(nameof(GetUsers), null, result);
    }
}
