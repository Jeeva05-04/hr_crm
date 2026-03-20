using System.ComponentModel.DataAnnotations;

namespace hr_crm.DTO
{
    public class LeadCreateDto
    {
        [Required] public string LeadName { get; set; } = string.Empty;
        [Required] public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class LeadAssignDto
    {
        [Required] public int AssignedToUserId { get; set; }
        [Required] public int AssignedByUserId { get; set; }
    }

    public class LeadUpdateStatusDto
    {
        [Required] public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class LeadResponseDto
    {
        public int LeadId { get; set; }
        public string LeadName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? AssignedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
