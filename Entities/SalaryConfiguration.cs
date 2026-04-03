namespace hr_crm.Entities
{
    public class SalaryConfiguration
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        // Monthly CTC amount
        public decimal MonthlyCTC { get; set; }

        // Standard allowances (configurable per employee)
        public decimal Conveyance { get; set; }
        public decimal MedicalAllowance { get; set; }

        // Legacy field kept for compatibility (monthly basic if previously used)
        public decimal BasicSalary { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int SetBy { get; set; }  // HR user who configured it
    }
}
