namespace hr_crm.DTO
{
    public class PayrollResponseDto
    {
        // Primary identifiers
        public int PayrollId { get; set; }
        public int UserId { get; set; } // Employee ID
        public string UserName { get; set; } = string.Empty; // NAME
        public DateTime Month { get; set; }
        public int Year { get; set; }

        // Employment details
        public string EmploymentType { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime? DOJ { get; set; }

        // CTC / payable
        public decimal MonthlyCTC { get; set; }
        public int NoOfPayableDays { get; set; }
        public decimal MonthlyCTCApportioned { get; set; }

        // Earnings breakdown
        public decimal BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal ConveyanceAllowance { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal TAOrPBonus { get; set; }
        public decimal GrossSalary { get; set; }

        // Payroll particulars
        public decimal EmployeePF { get; set; }
        public decimal PT { get; set; }
        public decimal EmployerPF { get; set; }
        public decimal TDS { get; set; }

        // Final
        public decimal NetPay { get; set; }
        // Backwards compatibility: some code references NetSalary
        public decimal NetSalary
        {
            get => NetPay;
            set => NetPay = value;
        }

        // Additional metadata
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays => WorkingDays - PresentDays;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        // Legacy / detailed fields
        public decimal TotalAllowances { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal OvertimePay { get; set; }
    }
}
