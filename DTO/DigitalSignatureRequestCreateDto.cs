using System.ComponentModel.DataAnnotations;

namespace hr_crm.DTO
{
    public class DigitalSignatureRequestCreateDto
    {
            [Required] public int UserId { get; set; }
            [Required] public int RequestedBy { get; set; }
            [Required] public string DocumentName { get; set; } = string.Empty;
            [Required] public string DocumentType { get; set; } = string.Empty;
            [Required] public IFormFile DocumentFile { get; set; }
          public string? Remarks { get; set; }
        
    }
}
