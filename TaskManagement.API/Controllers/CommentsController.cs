using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Extensions;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tasks/{taskId:int}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CommentDto>>> GetComments(int taskId)
        {
            var comments = await _commentService.GetCommentsAsync(taskId, User.GetUserId(), User.GetRole());
            return Ok(comments);
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> AddComment(int taskId, CreateCommentDto dto)
        {
            var comment = await _commentService.AddCommentAsync(taskId, dto, User.GetUserId(), User.GetRole());
            return Ok(comment);
        }
    }
}
