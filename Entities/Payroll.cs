using Microsoft.EntityFrameworkCore;

namespace hr_crm.Entities
{
    [Index(nameof(UserId), nameof(Month), IsUnique = true)]
    public class Payroll
    {
        public int PayrollId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime Month { get; set; }
        public int Year { get; set; }

        // Employment / employee info
        public string EmploymentType { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime? DOJ { get; set; }

        // CTC and payable days
        public decimal MonthlyCTC { get; set; }
        public int NoOfPayableDays { get; set; }
        public decimal MonthlyCTCApportioned { get; set; }

        // Earnings
        public decimal BasicSalary { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal TotalAllowances { get; set; }

        // Bonus
        public decimal BonusAmount { get; set; }

        // Detailed allowances
        public decimal HRA { get; set; }
        public decimal ConveyanceAllowance { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal TAOrPBonus { get; set; }

        public decimal GrossSalary { get; set; }

        // PF / Taxes / PT
        public decimal EmployeePF { get; set; }
        public decimal EmployerPF { get; set; }
        public decimal PT { get; set; }

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
