using System;
using System.Collections.Generic;

namespace hr_crm.DTO
{
    public class SendChatMessageDto
    {
        public string Content { get; set; } = string.Empty;
        public int? ReceiverUserId { get; set; } = null; // Null = broadcast to everyone
        public int? ConversationId { get; set; }
    }

    public class ChatMessageDto
    {
        public int ChatMessageId { get; set; }
        public int? ChatConversationId { get; set; }
        public int SentByUserId { get; set; }
        public string? SentByUserName { get; set; }
        public int? ReceiverUserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }

    public class UserContactDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }

    public class ChatPresenceDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string Status { get; set; } = "Available";
        public string? StatusMessage { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateChatPresenceDto
    {
        public string Status { get; set; } = "Available";
        public string? StatusMessage { get; set; }
    }

    public class CreateDirectConversationDto
    {
        public int UserId { get; set; }
    }

    public class CreateGroupConversationDto
    {
        public string Name { get; set; } = string.Empty;
        public List<int> MemberUserIds { get; set; } = new();
    }

    public class AddGroupMembersDto
    {
        public List<int> MemberUserIds { get; set; } = new();
    }

    public class ChatConversationMemberDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class ChatConversationDto
    {
        public int ChatConversationId { get; set; }
        public string ConversationType { get; set; } = string.Empty;
        public string? Name { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public List<ChatConversationMemberDto> Members { get; set; } = new();
        public ChatMessageDto? LastMessage { get; set; }
    }

    public class EmployeeContactDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
