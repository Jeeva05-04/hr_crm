using System.ComponentModel.DataAnnotations;

namespace hr_crm.Entities
{
    public class Lead
    {
        [Key]
        public int LeadId { get; set; }

        [Required]
        public string LeadName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        // Source: LinkedIn, Facebook, Instagram, Twitter, Website, Other
        public string Source { get; set; } = string.Empty;

        // Status: New, Contacted, Qualified, Assigned, Converted, Closed
        public string Status { get; set; } = "New";

        public string? Notes { get; set; }

        public int? AssignedToUserId { get; set; } // Employee the lead is assigned to

        public int? AssignedByUserId { get; set; } // HR who assigned it

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
