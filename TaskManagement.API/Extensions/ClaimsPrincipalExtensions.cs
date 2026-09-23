using System.Security.Claims;

namespace TaskManagement.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(id, out var parsed) ? parsed : 0;
        }

        public static string GetRole(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        }
    }
}
