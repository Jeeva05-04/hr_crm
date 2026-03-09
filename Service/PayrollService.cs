using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Service
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _repo;

            public PayrollService(IPayrollRepository repo)
            {
                _repo = repo;
            }

            public async Task<bool> GeneratePayrollAsync(PayrollCreateDto dto)
            {
                var gross = dto.BasicSalary + dto.Allowances;
                var net = gross - dto.Deductions;

                var payroll = new Payroll
                {
                    EmployeeId = dto.EmployeeId,
                    BasicSalary = dto.BasicSalary,
                    Allowances = dto.Allowances,
                    Deductions = dto.Deductions,
                    GrossSalary = gross,
                    NetSalary = net,
                    PayrollMonth = dto.PayrollMonth
                };

                await _repo.AddAsync(payroll);
                return true;
            }
        public Task<List<Payroll>> GetPayrollAsync()
         => _repo.GetPayrollAsync();

        public Task<List<Payroll>> GetPayrollAsync(int employeeId)
                => _repo.GetPayrollAsync(employeeId);

            public async Task<bool> UpdatePayrollAsync(int payrollId, PayrollCreateDto dto)
            {
                var gross = dto.BasicSalary + dto.Allowances;
                var net = gross - dto.Deductions;

                var payroll = new Payroll
                {
                    EmployeeId = dto.EmployeeId,
                    BasicSalary = dto.BasicSalary,
                    Allowances = dto.Allowances,
                    Deductions = dto.Deductions,
                    GrossSalary = gross,
                    NetSalary = net,
                    PayrollMonth = dto.PayrollMonth
                };

                return await _repo.UpdateAsync(payrollId, payroll);
            }

            public Task<bool> DeletePayrollAsync(int payrollId)
                => _repo.DeleteAsync(payrollId);
        
    }
}

