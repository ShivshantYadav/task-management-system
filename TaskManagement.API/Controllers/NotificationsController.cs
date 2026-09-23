using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Extensions;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var items = await _notificationService.GetForUserAsync(User.GetUserId());
            return Ok(items);
        }

        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notificationService.MarkReadAsync(id, User.GetUserId());
            return NoContent();
        }
    }
}
