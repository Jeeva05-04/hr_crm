
using System;
namespace hr_crm.Entities
{
    
    public class EmployeeTraining
    {
        
        public int Id { get; set; }

     
        public int UserId { get; set; }

        public string TrainingName { get; set; }

        public string Description { get; set; }

        public bool IsMandatory { get; set; } = false;

 
        public string Status { get; set; } = "Assigned";
        // Assigned, InProgress, Completed

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletionDate { get; set; }

        public DateTime? DueDate { get; set; }

        public string? TrainingProvider { get; set; }

        // Training category (Technical / SoftSkill / Compliance)
        public string? Category { get; set; }

        // Total duration in hours
        public int? DurationHours { get; set; }

        // Progress percentage
        public int Progress { get; set; } = 0;

        // Assigned by manager or HR
        public int AssignedBy { get; set; }

        public bool IsCertified { get; set; } = false;

        // Employee feedback after training
        public string? Feedback { get; set; }

        // Training score / result
        public int? Score { get; set; }

        // Created time
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Last updated time
        public DateTime? UpdatedAt { get; set; }
    }
}


