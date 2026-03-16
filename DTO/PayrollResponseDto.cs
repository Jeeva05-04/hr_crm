namespace hr_crm.DTO
{
    public class PayrollResponseDto
    {
        public int PayrollId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime Month { get; set; }
        public int Year { get; set; }

        // Earnings
        public decimal BasicSalary { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal TotalAllowances { get; set; }

        // Deductions
        public decimal AbsentDeduction { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal TotalDeductions { get; set; }

        // Attendance
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays => WorkingDays - PresentDays;

        // Final
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }
}
