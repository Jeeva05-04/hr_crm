using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hr_crm.Service;
using hr_crm.Authorization;

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

        [HasPermission("NOTIFICATION_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = int.Parse(User.FindFirst("sub")!.Value);

            var notifications = await _service.GetNotifications(userId);

            return Ok(notifications);
        }

        [HasPermission("NOTIFICATION_UPDATE")]
        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _service.MarkAsRead(id);

            return Ok("Notification marked as read");
        }
    }
}