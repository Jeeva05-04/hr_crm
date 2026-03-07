using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IPayrollService
    {

        Task<bool> GeneratePayrollAsync(PayrollCreateDto dto);

        Task<List<Payroll>> GetPayrollAsync();

        Task<List<Payroll>> GetPayrollAsync(int employeeId);

        Task<bool> UpdatePayrollAsync(int payrollId, PayrollCreateDto dto);

        Task<bool> DeletePayrollAsync(int payrollId);
     
    }


}

