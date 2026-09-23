using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Tests
{
    // Lightweight stand-in for JwtTokenGenerator so service tests don't depend on IConfiguration.
    public class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public (string Token, DateTime ExpiresAt) GenerateToken(User user)
            => ($"fake-token-for-{user.Id}", DateTime.UtcNow.AddHours(1));
    }
}
