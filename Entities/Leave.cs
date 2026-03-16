using System;
namespace hr_crm.Entities
{
    public class Leave
    {
        public int LeaveId { get; set; }
        public int UserId { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ApprovedBY { get; set; } = string.Empty;
        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;
        public int TotalDays { get; set; }  // calculated excluding weekends + holidays
    }
}