using System.ComponentModel.DataAnnotations;

namespace hr_crm.Models
{
    public class KnowledgeCreateDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "BranchId must be valid")]
        public int BranchId { get; set; }

        [Required]
        [StringLength(50)]
        public string RecordType { get; set; }   // Policy / SOP / FAQ

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(100)]
        public string SubCategory { get; set; }

        [Required]
        [StringLength(1000)]
        public string Summary { get; set; }

        [Required]
        public string ApprovalStatus { get; set; }   // Pending / Approved

        public string ApprovedBy { get; set; }

        [Required]
        public string Visibility { get; set; }   // All / HR / Manager

        [Required]
        public string Status { get; set; }        // Active / Inactive

        [Required]
        public int CreatedBy { get; set; }
    }
}
