namespace hr_crm.DTO
{
    public class RecruitmentDashboardDto
    {
        public int TotalOpenings { get; set; }
        public int TotalApplicants { get; set; }
        public StatusBreakdownDto ByStatus { get; set; } = new();
        public List<DepartmentStatsDto> ByDepartment { get; set; } = new();
        public List<RoleStatsDto> ByRole { get; set; } = new();
    }

    public class StatusBreakdownDto
    {
        public int Applied { get; set; }
        public int Screening { get; set; }
        public int InterviewScheduled { get; set; }
        public int OnHold { get; set; }
        public int Selected { get; set; }
        public int Offered { get; set; }
        public int Hired { get; set; }
        public int Rejected { get; set; }
    }

    public class DepartmentStatsDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int Openings { get; set; }
        public int Applicants { get; set; }
        public int InterviewsDone { get; set; }
        public int OnHold { get; set; }
        public int Selected { get; set; }
        public int Hired { get; set; }
        public int Rejected { get; set; }
    }

    public class RoleStatsDto
    {
        public string Role { get; set; } = string.Empty;
        public int Openings { get; set; }
        public int Applicants { get; set; }
        public int InterviewsDone { get; set; }
        public int OnHold { get; set; }
        public int Selected { get; set; }
        public int Offered { get; set; }
        public int Hired { get; set; }
        public int Rejected { get; set; }
    }
}
