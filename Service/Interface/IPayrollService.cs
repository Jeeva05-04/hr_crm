using hr_crm.DTO;

namespace hr_crm.Service.Interface
{
    public interface IPayrollService
    {
        Task<PayrollResponseDto> GeneratePayrollAsync(PayrollCreateDto dto);
        Task<List<PayrollResponseDto>> GetAllPayrollAsync();
        Task<List<PayrollResponseDto>> GetPayrollByUserIdAsync(int userId);
        Task<PayrollResponseDto?> UpdatePayrollAsync(int payrollId, PayrollCreateDto dto);
        Task<bool> DeletePayrollAsync(int payrollId);
        Task AddAllowanceAsync(AllowanceCreateDto dto);
        Task AddDeductionAsync(DeductionCreateDto dto);
        Task<PayslipDto?> GetCurrentPayslipAsync(int userId);
    }
}