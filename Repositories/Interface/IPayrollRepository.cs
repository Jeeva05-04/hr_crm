using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IPayrollRepository
    {

        Task<List<Payroll>> GetPayrollAsync();

        Task<List<Payroll>> GetPayrollAsync(int employeeId);

            Task<Payroll?> GetByIdAsync(int payrollId);

            Task AddAsync(Payroll payroll);

            Task<bool> UpdateAsync(int payrollId, Payroll payroll);

            Task<bool> DeleteAsync(int payrollId);
        
    }
}

