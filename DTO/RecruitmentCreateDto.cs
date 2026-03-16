namespace hr_crm.Models
{
    public class RecruitmentCreateDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string AppliedPosition { get; set; } = null!;
        public int DepartmentId { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Source { get; set; } = null!;   // LinkedIn / Referral / Website / Direct
        public decimal? ExpectedSalary { get; set; }
        public IFormFile? Resume { get; set; }        // PDF upload
    }

    public class ScheduleInterviewDto
    {
        public DateTime InterviewDate { get; set; }
        public string InterviewerName { get; set; } = null!;
        public string InterviewType { get; set; } = null!;   // Phone / Video / In-Person
        public string? Notes { get; set; }
    }

    public class UpdateStatusDto
    {
        // Applied | Screening | InterviewScheduled | Selected | Offered | Onboarded | Rejected
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
        public decimal? OfferedSalary { get; set; }
    }

    public class AssignLeadDto
    {
        public int AssignedToUserId { get; set; }
    }
}

