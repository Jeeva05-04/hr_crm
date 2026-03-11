namespace hr_crm.DTO
{
    public class TrainingResponseDto
    {
    
            public int Id { get; set; }
            public int UserId { get; set; }
            public string TrainingName { get; set; }
            public string Description { get; set; }
            public bool IsMandatory { get; set; }
            public string Status { get; set; }
            public DateTime AssignedDate { get; set; }
            public DateTime? DueDate { get; set; }
            public DateTime? CompletionDate { get; set; }
            public int Progress { get; set; }
            public string? TrainingProvider { get; set; }
            public string? Category { get; set; }
            public int? DurationHours { get; set; }
            public int AssignedBy { get; set; }
            public bool IsCertified { get; set; }
            public string? Feedback { get; set; }
            public int? Score { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
    }
}

