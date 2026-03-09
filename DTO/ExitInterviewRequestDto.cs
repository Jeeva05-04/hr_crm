namespace hr_crm.DTO
{
    public class ExitInterviewRequestDto
    {
       
            public int EmployeeId { get; set; }

            public DateTime ScheduledDate { get; set; }
            public string? ReasonForLeaving { get; set; }

    }
}

