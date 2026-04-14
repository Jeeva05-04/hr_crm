using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IPayrollRepository
    {
        Task<Payroll> GeneratePayrollAsync(Payroll payroll);
        Task<List<Payroll>> GetAllPayrollAsync();
        Task<List<Payroll>> GetPayrollByUserIdAsync(int userId);
        Task<Payroll?> GetPayrollByIdAsync(int payrollId);
        Task<Payroll?> UpdatePayrollAsync(Payroll payroll);
        Task<bool> DeletePayrollAsync(int payrollId);

        Task AddAllowanceAsync(Allowance allowance);
        Task AddDeductionAsync(Deduction deduction);
        Task<List<Allowance>> GetAllowancesAsync(int userId, DateTime month, int year);
        Task<List<Deduction>> GetDeductionsAsync(int userId, DateTime month, int year);

        Task<Payroll?> GetPayrollByUserMonthYearAsync(int userId, DateTime month, int year);

        // Overtime integration
        Task<double> GetOvertimeHoursForMonthAsync(int userId, int year, int month);

        // Attendance integration
        Task<int> GetPresentDaysForMonthAsync(int userId, int year, int month);

        // Approval workflow
        Task<bool> ApprovePayrollAsync(int payrollId, int approvedBy);
        Task<bool> MarkAsPaidAsync(int payrollId);

        // Bonus
        Task<Bonus> CreateBonusAsync(Bonus bonus);
        Task<List<Bonus>> GetBonusesByUserAsync(int userId);
        Task<List<Bonus>> GetAllBonusesAsync();
        Task<Bonus?> GetBonusByIdAsync(int id);
        Task<bool> ApproveBonusAsync(int id, int approvedBy);
        Task<List<Bonus>> GetApprovedBonusesForMonthAsync(int userId, int month, int year);

        // Salary Configuration
        Task<SalaryConfiguration> SetSalaryConfigAsync(SalaryConfiguration config);
        Task<SalaryConfiguration?> GetSalaryConfigAsync(int userId);
        Task<List<SalaryConfiguration>> GetAllSalaryConfigsAsync();
    }
}
