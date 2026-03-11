using System.ComponentModel.DataAnnotations;

namespace hr_crm.DTO
{
    public class DigitalSignatureSignCreateDto
    {
       
            [Required] public int UserId { get; set; }
            public string? SignedByIp { get; set; }
            public string? Remarks { get; set; }
        
    }
}
