using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IPayrollService
    {
        Task<(PayrollResponseDto? Result, string? Error)> GeneratePayrollAsync(PayrollCreateDto dto);
        Task<List<PayrollResponseDto>> GetAllPayrollAsync();
        Task<List<PayrollResponseDto>> GetPayrollByUserIdAsync(int userId);
        Task<PayrollResponseDto?> UpdatePayrollAsync(int payrollId, PayrollCreateDto dto);
        Task<bool> DeletePayrollAsync(int payrollId);
        Task AddAllowanceAsync(AllowanceCreateDto dto);
        Task AddDeductionAsync(DeductionCreateDto dto);
        Task<PayslipDto?> GetCurrentPayslipAsync(int userId);
        Task<(bool Success, string? Error)> ApprovePayrollAsync(int payrollId, int approvedBy);
        Task<(bool Success, string? Error)> MarkAsPaidAsync(int payrollId);

        // Bonus
        Task<(Bonus? Result, string? Error)> CreateBonusAsync(BonusCreateDto dto, int createdBy);
        Task<List<Bonus>> GetBonusesByUserAsync(int userId);
        Task<List<Bonus>> GetAllBonusesAsync();
        Task<(bool Success, string? Error)> ApproveBonusAsync(int id, int approvedBy);

        // Salary Configuration
        Task<SalaryConfiguration> SetSalaryConfigAsync(SalaryConfigDto dto, int setBy);
        Task<SalaryConfiguration?> GetSalaryConfigAsync(int userId);
        Task<List<SalaryConfiguration>> GetAllSalaryConfigsAsync();

        // Auto payroll generation (called by background job)
        Task<(int Generated, int Skipped)> AutoGeneratePayrollForAllAsync();
    }
}
