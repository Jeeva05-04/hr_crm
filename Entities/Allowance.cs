namespace hr_crm.Entities
{
    public class Allowance
    {
        public int AllowanceId { get; set; }
        public int UserId { get; set; }
        public string AllowanceType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Month { get; set; }
        public int Year { get; set; }
    }
}
