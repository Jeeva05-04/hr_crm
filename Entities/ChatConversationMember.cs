using System;

namespace hr_crm.Entities
{
    public class ChatConversationMember
    {
        public int ChatConversationMemberId { get; set; }
        public int ChatConversationId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
