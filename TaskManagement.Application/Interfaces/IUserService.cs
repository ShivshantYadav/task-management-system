using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetUsersAsync(int requestingUserId, string requestingUserRole, string? role = null);

    /// <summary>Admin-only: create a Manager or User account and email its temporary password.</summary>
    Task<UserCreationResultDto> CreateUserAsync(CreateUserRequestDto dto, int requestingUserId, string requestingUserRole);
}
