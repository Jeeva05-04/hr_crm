using System;

namespace hr_crm.DTO
{
    public class LogEntryDto
    {
        public int LogEntryId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
