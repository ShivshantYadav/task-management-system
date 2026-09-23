using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto> AddCommentAsync(int taskId, CreateCommentDto dto, int requestingUserId, string requestingUserRole);
        Task<List<CommentDto>> GetCommentsAsync(int taskId, int requestingUserId, string requestingUserRole);
    }
}
