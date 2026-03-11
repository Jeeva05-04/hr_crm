namespace hr_crm.Entities
{
    public class Deduction
    {
        public int DeductionId { get; set; }
        public int UserId { get; set; }
        public string DeductionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Month { get; set; } 
        public int Year { get; set; }
    }
}
