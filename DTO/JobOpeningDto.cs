using System.ComponentModel.DataAnnotations;

namespace hr_crm.DTO
{
    public class JobOpeningCreateDto
    {
        [Required] public string Title { get; set; } = string.Empty;
        [Required] public int DepartmentId { get; set; }
        [Required] public int TotalOpenings { get; set; } = 1;
        public string? Description { get; set; }
    }

    public class JobOpeningUpdateDto
    {
        public string? Title { get; set; }
        public int? TotalOpenings { get; set; }
        public string? Description { get; set; }
        // Open | Closed | Paused
        public string? Status { get; set; }
    }

    public class JobOpeningResponseDto
    {
        public int JobOpeningId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalOpenings { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        // Derived — how many candidates have been hired against this opening
        public int FilledCount { get; set; }
        public int RemainingOpenings { get; set; }
    }
}
