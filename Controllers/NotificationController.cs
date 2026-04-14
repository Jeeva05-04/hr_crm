using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hr_crm.Service;
using hr_crm.Authorization;
using System.Security.Claims;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _service;

        public NotificationController(NotificationService service)
        {
            _service = service;
        }

        // GET all notifications for logged-in user
        [HasPermission("NOTIFICATION_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            var notifications = await _service.GetNotifications(userId);
            return Ok(notifications);
        }

        // GET unread count (for badge on frontend)
        [HasPermission("NOTIFICATION_VIEW")]
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            var count = await _service.GetUnreadCount(userId);
            return Ok(new { UnreadCount = count });
        }

        // Mark single notification as read
        [HasPermission("NOTIFICATION_UPDATE")]
        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _service.MarkAsRead(id);
            return Ok(new { Message = "Notification marked as read." });
        }

        // Mark ALL notifications as read
        [HasPermission("NOTIFICATION_UPDATE")]
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            await _service.MarkAllAsRead(userId);
            return Ok(new { Message = "All notifications marked as read." });
        }

        // Delete single notification
        [HasPermission("NOTIFICATION_VIEW")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            var deleted = await _service.DeleteNotification(id, userId);
            if (!deleted) return NotFound("Notification not found.");
            return Ok(new { Message = "Notification deleted." });
        }

        // Delete ALL notifications for logged-in user
        [HasPermission("NOTIFICATION_VIEW")]
        [HttpDelete("clear-all")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            await _service.DeleteAllNotifications(userId);
            return Ok(new { Message = "All notifications cleared." });
        }
    }
}
