namespace hr_crm.DTO
{
    public class DigitalSignatureResponseCreateDto
    {
   
            public int SignatureId { get; set; }
            public int UserId { get; set; }
            public int RequestedBy { get; set; }
            public string DocumentName { get; set; } = string.Empty;
            public string DocumentType { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string? FilePath { get; set; }
            public string? SignedFilePath { get; set; }
            public string? SignatureHash { get; set; }
            public string? SignedByIp { get; set; }
            public DateTime RequestedAt { get; set; }
            public DateTime? SignedAt { get; set; }
            public string? Remarks { get; set; }
        
    }
}
