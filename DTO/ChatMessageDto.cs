using System;
using System.Collections.Generic;

namespace hr_crm.DTO
{
    public class SendChatMessageDto
    {
        public string Content { get; set; } = string.Empty;
        public int? ReceiverUserId { get; set; } = null; // Null = broadcast to everyone
    }

    public class ChatMessageDto
    {
        public int ChatMessageId { get; set; }
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
}
