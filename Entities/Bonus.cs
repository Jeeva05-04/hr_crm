namespace hr_crm.Entities
{
    public class Bonus
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public string BonusType { get; set; } = string.Empty;  // Festival | Performance | Annual
        public decimal Amount { get; set; }
        public string? Reason { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        // Pending → Approved (only approved bonuses are included in payroll)
        public string Status { get; set; } = "Pending";

        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }
}
