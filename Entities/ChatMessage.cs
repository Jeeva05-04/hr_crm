using System;

namespace hr_crm.Entities
{
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }

        public int SentByUserId { get; set; }
        public string? SentByUserName { get; set; }

        // Null = broadcast to everyone, specific ID = direct message
        public int? ReceiverUserId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
    }
}
