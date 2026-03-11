namespace hr_crm.Entities
{
    public class WorkExperience
    {
        public int Id { get; set; }

        public int EmployeeOnboardingId { get; set; }

        public string PreviousCompanyDetails { get; set; }

        public string OfferedDesignation { get; set; }

        public decimal OfferedSalaryNTH { get; set; }

        public decimal OfferedMonthlyCTC { get; set; }

        public decimal OfferedYearlyCTC { get; set; }

        public string TotalExperience { get; set; }

        public string LastCompanyPFNumber { get; set; }

        public string LastCompanyUAN { get; set; }

        public string? PreviousCompanyPayslipPath { get; set; }
    }
}