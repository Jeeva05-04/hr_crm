namespace hr_crm.DTO
{
    public class AllowanceCreateDto
    {
        public int UserId { get; set; }
        public string AllowanceType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
       
    }
}