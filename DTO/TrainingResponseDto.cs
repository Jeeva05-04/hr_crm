namespace hr_crm.DTO
{
    public class TrainingResponseDto
    {
    
            public int Id { get; set; }
            public int EmployeeId { get; set; }
            public string TrainingName { get; set; }
            public string Description { get; set; }
            public bool IsMandatory { get; set; }
            public string Status { get; set; }
            public DateTime AssignedDate { get; set; }
            public DateTime? CompletionDate { get; set; }
        
    }
}

