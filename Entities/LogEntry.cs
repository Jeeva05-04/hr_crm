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

        // Structured fields for easier querying and reporting
        // HTTP status code returned by the request (if applicable)
        public int? StatusCode { get; set; }

        // Duration in milliseconds of the action
        public int? DurationMs { get; set; }

        // Controller and action names for structured queries
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }

        // Truncated user agent string (optional)
        public string? UserAgent { get; set; }
    }
}
