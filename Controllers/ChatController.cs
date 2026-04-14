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

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployeeContacts()
        {
            try
            {
                var contacts = await _chatService.GetEmployeeContactsAsync();
                return Ok(new { success = true, employees = contacts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee contacts");
                return StatusCode(500, new { message = "Error retrieving employee contacts" });
            }
        }

        [HttpPost("conversations/direct")]
        public async Task<IActionResult> CreateDirectConversation([FromBody] CreateDirectConversationDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var (result, error) = await _chatService.CreateDirectConversationAsync(userId.Value, GetCurrentUserName(userId.Value), dto.UserId);
            if (result == null)
                return BadRequest(new { message = error });

            return Ok(new { success = true, conversation = result });
        }

        [HttpPost("conversations/group")]
        public async Task<IActionResult> CreateGroupConversation([FromBody] CreateGroupConversationDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var (result, error) = await _chatService.CreateGroupConversationAsync(userId.Value, GetCurrentUserName(userId.Value), dto);
            if (result == null)
                return BadRequest(new { message = error });

            return Ok(new { success = true, conversation = result });
        }

        [HttpPost("conversations/{conversationId}/members")]
        public async Task<IActionResult> AddGroupMembers(int conversationId, [FromBody] AddGroupMembersDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var (success, error) = await _chatService.AddGroupMembersAsync(userId.Value, GetCurrentUserName(userId.Value), conversationId, dto);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { success = true, conversationId });
        }

        [HttpDelete("conversations/{conversationId}")]
        public async Task<IActionResult> DeleteConversation(int conversationId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var (success, error) = await _chatService.DeleteGroupConversationAsync(userId.Value, conversationId);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { success = true, conversationId, message = "Group conversation deleted permanently." });
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            try
            {
                var conversations = await _chatService.GetConversationsAsync(userId.Value);
                return Ok(new { success = true, conversations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversations");
                return StatusCode(500, new { message = "Error retrieving conversations" });
            }
        }

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<IActionResult> GetConversationMessages(int conversationId, [FromQuery] int pageSize = 100, [FromQuery] int pageNumber = 1)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            try
            {
                var (messages, error) = await _chatService.GetConversationMessagesAsync(userId.Value, conversationId, pageSize, pageNumber);
                if (messages == null)
                    return NotFound(new { message = error });

                return Ok(new
                {
                    success = true,
                    conversationId,
                    totalCount = messages.Count,
                    pageSize,
                    pageNumber,
                    messages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation messages");
                return StatusCode(500, new { message = "Error retrieving conversation messages" });
            }
        }

        // POST: /api/chat/send
        // Body: { "content": "message", "conversationId": 1 }
        // Body: { "content": "message", "receiverUserId": 5 } (auto-creates/reuses a direct conversation)
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            if (dto == null)
                return BadRequest(new { message = "Request body is required" });

            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Message content cannot be empty" });

            var userName = GetCurrentUserName(userId.Value);

            try
            {
                var message = await _chatService.SendMessageAsync(dto, userId.Value, userName);
                return Ok(new
                {
                    success = true,
                    message.ChatMessageId,
                    message.ChatConversationId,
                    message.SentByUserId,
                    message.SentByUserName,
                    message.ReceiverUserId,
                    message.Content,
                    message.SentAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new { message = "Error sending message: " + ex.Message });
            }
        }

        // Legacy endpoint retained for compatibility.
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
                        m.ChatConversationId,
                        m.SentByUserId,
                        m.SentByUserName,
                        m.ReceiverUserId,
                        m.Content,
                        m.SentAt
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
        public async Task<IActionResult> GetStatus([FromQuery] int? userId = null)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue) return Unauthorized();

            var targetUserId = userId.GetValueOrDefault(currentUserId.Value);
            if (targetUserId <= 0)
                return BadRequest(new { message = "userId must be greater than 0." });

            try
            {
                var userName = targetUserId == currentUserId.Value
                    ? GetCurrentUserName(currentUserId.Value)
                    : null;

                var status = await _chatService.EnsurePresenceAsync(targetUserId, userName);
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
