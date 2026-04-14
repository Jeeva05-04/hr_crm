namespace hr_crm.DTO
{
    public class AssignTrainingDto
    {

        public int UserId { get; set; }
        public string TrainingName { get; set; }
        public string Description { get; set; }
        public bool IsMandatory { get; set; }
        public string? TrainingProvider { get; set; }
        public string? Category { get; set; }
        public int? DurationHours { get; set; }
        public int AssignedBy { get; set; }
        public DateTime? DueDate { get; set; }

    }
    
}

