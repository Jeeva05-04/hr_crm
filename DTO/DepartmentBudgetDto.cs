namespace hr_crm.DTO
{
    public class DepartmentBudgetDto
    {
        public int DepartmentId { get; set; }
        public decimal TotalAnnualBudget { get; set; }
        public decimal TrainingBudget { get; set; }
        public decimal ResourceBudget { get; set; }
        public int Year { get; set; }
    }
}