using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hr_crm.Entities
{
    public class DigitalSignature
    {
  
            [Key]
            public int SignatureId { get; set; }

            [Required]
            public int  UserId { get; set; }

            [Required]
            public int RequestedBy { get; set; } // Manager's EmployeeId

            [Required]
            public string DocumentName { get; set; } = string.Empty;

            public string DocumentType { get; set; } = string.Empty; // e.g. "Offer Letter", "Contract"

            public string Status { get; set; } = "Pending"; // Pending, Signed, Rejected

            public string? SignatureHash { get; set; } // Hash generated on signing
          
            public string? SignedByIp { get; set; } // IP address of signer

            public DateTime RequestedAt { get; set; } = DateTime.UtcNow; 

            public DateTime? SignedAt { get; set; }

            public string? Remarks { get; set; }
        
    }
}

