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
    }
}