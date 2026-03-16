namespace hr_crm.DTO
{
    public class BonusCreateDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string BonusType { get; set; } = string.Empty;  // Festival | Performance | Annual
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
