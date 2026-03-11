namespace hr_crm.DTO
{
    public class DeductionCreateDto
    {
        public int UserId { get; set; }
        public string DeductionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        
    }
}