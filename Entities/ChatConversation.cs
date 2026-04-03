using System;

namespace hr_crm.Entities
{
    public class ChatConversation
    {
        public int ChatConversationId { get; set; }
        public string ConversationType { get; set; } = "Direct";
        public string? Name { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
