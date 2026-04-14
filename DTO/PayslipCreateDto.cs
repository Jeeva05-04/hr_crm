namespace hr_crm.DTO
{
    public class PayslipDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime Month { get; set; }
        public int Year { get; set; }

        // Attendance summary
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays => WorkingDays - PresentDays;

        // Earnings breakdown
        public decimal BasicSalary { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal BonusAmount { get; set; }
        public string? BonusDetails { get; set; }   // e.g. "Festival Bonus + Performance Bonus"
        public List<AllowanceItemDto> Allowances { get; set; } = new();
        public decimal TotalAllowances { get; set; }

        // Deductions breakdown
        public decimal AbsentDeduction { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal AnnualSalary { get; set; }   // for tax transparency
        public List<DeductionItemDto> Deductions { get; set; } = new();
        public decimal TotalDeductions { get; set; }

        // Final
        public decimal GrossEarnings => BasicSalary + OvertimePay + BonusAmount + TotalAllowances;
        public decimal TotalDeductionsAll => AbsentDeduction + TaxDeduction + TotalDeductions;
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
    }

    public class AllowanceItemDto
    {
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class DeductionItemDto
    {
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
