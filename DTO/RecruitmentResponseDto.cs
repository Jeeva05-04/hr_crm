namespace hr_crm.DTO
{
    public class RecruitmentResponseDto
    {
        public int CandidateId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string AppliedPosition { get; set; } = null!;
        public int DepartmentId { get; set; }
        public string ApplicationDate { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Source { get; set; } = null!;

        // Interview
        public DateTime? InterviewDate { get; set; }
        public string? InterviewerName { get; set; }
        public string? InterviewType { get; set; }
        public string? InterviewNotes { get; set; }

        // Salary
        public decimal? ExpectedSalary { get; set; }
        public decimal? OfferedSalary { get; set; }

        // Resume & Onboarding
        public string? ResumeUrl { get; set; }
        public int? OnboardingId { get; set; }

        // Lead assignment
        public int? AssignedToUserId { get; set; }

        // Job opening link
        public int? JobOpeningId { get; set; }
    }
}
