namespace hr_crm.DTO
{
    public class PayrollResponseDto
    {
        public int PayrollId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime Month { get; set; }
        public int Year { get; set; }

        // Company-specific columns (matching requested headers)
        // S.NO is typically provided by the client when listing, so not included here.
        public string EmploymentType { get; set; } = string.Empty; // "Employement Type"
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime? DOJ { get; set; }

        public decimal MonthlyCTC { get; set; }
        public int NoOfPayableDays { get; set; }
        public decimal MonthlyCTCApportioned { get; set; }

        // Earnings breakdown
        public decimal BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal ConveyanceAllowance { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal TAOrPBonus { get; set; } // "T.A / P.Bonus"

        public decimal GrossSalary { get; set; }

        // Deductions
        public decimal EmployeePF { get; set; }
        public decimal PT { get; set; }
        public decimal EmployerPF { get; set; }
        public decimal TDS { get; set; }

        // Final
        public decimal NetPay { get; set; }
        // Backwards compatibility: some controllers still reference NetSalary
        public decimal NetSalary
        {
            get => NetPay;
            set => NetPay = value;
        }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }
}
