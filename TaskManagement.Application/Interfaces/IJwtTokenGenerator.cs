using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime ExpiresAt) GenerateToken(User user);
    }
}
