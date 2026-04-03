using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using hr_crm.DTO;
using hr_crm.Extensions;
using hr_crm.Service.Interface;
using System.Security.Claims;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        // POST: /api/chat/send
        // Send a message (broadcast to everyone or to specific user)
        // Body: { "content": "message", "receiverUserId": null } (null or omit for broadcast)
        // Body: { "content": "message", "receiverUserId": 5 } (for direct message)
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            if (dto == null)
                return BadRequest(new { message = "Request body is required" });

            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Message content cannot be empty" });

            // Get user display name from token claims
            var userName = GetCurrentUserName(userId.Value);

            try
            {
                var message = await _chatService.SendMessageAsync(dto, userId.Value, userName);
                return Ok(new
                {
                    success = true,
                    message.ChatMessageId,
                    message.SentByUserId,
                    message.SentByUserName,
                    message.ReceiverUserId,
                    message.Content,
                    message.SentAt,
                    messageType = message.ReceiverUserId.HasValue ? "direct" : "broadcast"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new { message = "Error sending message: " + ex.Message });
            }
        }

        // GET: /api/chat/history
        // Get all chat messages (Chat History for everyone)
        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory([FromQuery] int pageSize = 100, [FromQuery] int pageNumber = 1)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            try
            {
                var messages = await _chatService.GetMessagesAsync(userId.Value, pageSize, pageNumber);
                return Ok(new
                {
                    success = true,
                    totalCount = messages.Count,
                    pageSize,
                    pageNumber,
                    messages = messages.Select(m => new
                    {
                        m.ChatMessageId,
                        m.SentByUserId,
                        m.SentByUserName,
                        m.ReceiverUserId,
                        m.Content,
                        m.SentAt,
                        messageType = m.ReceiverUserId.HasValue ? $"Direct to User {m.ReceiverUserId}" : "Broadcast (Everyone)"
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat history");
                return StatusCode(500, new { message = "Error retrieving chat history" });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetMyStatus()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            try
            {
                var status = await _chatService.EnsurePresenceAsync(userId.Value, GetCurrentUserName(userId.Value));
                return Ok(new { success = true, presence = status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat status");
                return StatusCode(500, new { message = "Error retrieving chat status" });
            }
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateMyStatus([FromBody] UpdateChatPresenceDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            if (dto == null)
                return BadRequest(new { message = "Request body is required" });

            try
            {
                var status = await _chatService.UpdatePresenceAsync(userId.Value, GetCurrentUserName(userId.Value), dto);
                return Ok(new { success = true, presence = status });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chat status");
                return StatusCode(500, new { message = "Error updating chat status" });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (int.TryParse(userIdClaim, out var userId))
                return userId;
            return null;
        }

        private string GetCurrentUserName(int userId)
        {
            return User.GetDisplayName()
                ?? User.FindFirst("username")?.Value
                ?? User.FindFirst("email")?.Value
                ?? $"User{userId}";
        }
    }
}
