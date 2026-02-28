namespace hr_crm.Entities
{
    public class DepartmentBudget
    {
        public int Id { get; set; }

        public int DepartmentId { get; set; }

        public decimal TotalAnnualBudget { get; set; }
        public decimal TrainingBudget { get; set; }
        public decimal ResourceBudget { get; set; }

        public decimal UsedBudget { get; set; }

        public int Year { get; set; }

        public string Status { get; set; } = "Draft";

        public decimal? ApprovedAmount { get; set; }

        public int? HeadApprovedBy { get; set; }
        public DateTime? HeadApprovedDate { get; set; }

        public int? FinanceApprovedBy { get; set; }
        public DateTime? FinanceApprovedDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}