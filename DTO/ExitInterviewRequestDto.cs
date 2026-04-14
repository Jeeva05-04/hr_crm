namespace hr_crm.DTO
{
    public class ExitInterviewRequestDto
    {
       
            public int  UserId { get; set; }

            public DateTime ScheduledDate { get; set; }
            public string? ReasonForLeaving { get; set; }

    }
}

