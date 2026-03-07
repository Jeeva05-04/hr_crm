namespace hr_crm.DTO
{
    public class ExitInterviewResponseDto
    {
        
            public int Id { get; set; }

            public int EmployeeId { get; set; }

            public DateTime ScheduledDate { get; set; }

            public string? ReasonForLeaving { get; set; }

            public string? Feedback { get; set; }

            public string? Suggestions { get; set; }

            public string Status { get; set; }
        
    }
}

