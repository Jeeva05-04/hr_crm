using System;

namespace hr_crm.Entities
{
    public class LeaveType
    {
        public int LeaveTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
