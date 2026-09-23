using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(int requestingUserId, string requestingUserRole);
    }
}
