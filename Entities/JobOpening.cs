using System.ComponentModel.DataAnnotations;

namespace hr_crm.Entities
{
    public class JobOpening
    {
        [Key]
        public int JobOpeningId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty; // Role name e.g. "Software Engineer"

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int TotalOpenings { get; set; } = 1; // Number of seats available

        public string? Description { get; set; }

        // Open | Closed | Paused
        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }

        public virtual Department Department { get; set; } = null!;
    }
}
