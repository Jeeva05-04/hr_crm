namespace hr_crm.Entities
{
    public class Payroll
    {
        public int PayrollId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime Month { get; set; }
        public int Year { get; set; }

        // Earnings
        public decimal BasicSalary { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal TotalAllowances { get; set; }

        // Bonus
        public decimal BonusAmount { get; set; }

        // Deductions
        public decimal AbsentDeduction { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal TotalDeductions { get; set; }

        // Attendance summary
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }

        // Final
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime CreatedDate { get; set; }

        // Approval
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }
}
