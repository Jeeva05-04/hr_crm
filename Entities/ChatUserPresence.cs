using System;

namespace hr_crm.Entities
{
    public class ChatUserPresence
    {
        public int ChatUserPresenceId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string Status { get; set; } = "Available";
        public string? StatusMessage { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
