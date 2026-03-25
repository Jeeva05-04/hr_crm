using System;

namespace hr_crm.Entities
{
    public class LogEntry
    {
        public int LogEntryId { get; set; }

        // The user who performed the action — nullable for anonymous
        public int? UserId { get; set; }

        // Short username or identifier
        public string? UserName { get; set; }

        // HTTP method and path or a simple action description
        public string Action { get; set; } = string.Empty;

        // Optional details (request query, body summary, etc.)
        public string? Details { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
