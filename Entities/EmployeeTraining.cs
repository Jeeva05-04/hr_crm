
using System;
namespace hr_crm.Entities
{
    
    public class EmployeeTraining
    {
        
        public int Id { get; set; }

     
        public int EmployeeId { get; set; }

        public string TrainingName { get; set; }

        public string Description { get; set; }

        public bool IsMandatory { get; set; } = false;

 
        public string Status { get; set; } = "Assigned";
        // Assigned, InProgress, Completed

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletionDate { get; set; }

    }
}

