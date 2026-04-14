using System;

namespace hr_crm.Entities
{
    public class ExitInterview
    {
        public int Id { get; set; }

        public int  UserId { get; set; }

        public DateTime ScheduledDate { get; set; }

        public string? ReasonForLeaving { get; set; }

        public string? Feedback { get; set; }

        public string? Suggestions { get; set; }

        public string Status { get; set; }
    }
}