using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class PayrollRepository : IPayrollRepository
    {
            private readonly AppDbContext _context;

            public PayrollRepository(AppDbContext context)
            {
                _context = context;
            }


        public async Task<List<Payroll>> GetPayrollAsync()
        {
            return await _context.Payrolls
                .OrderByDescending(p => p.PayrollMonth)
                .ToListAsync();
        }

        public async Task<List<Payroll>> GetPayrollAsync(int employeeId)
            {
                return await _context.Payrolls
                    .Where(p => p.EmployeeId == employeeId)
                    .OrderByDescending(p => p.PayrollMonth)
                    .ToListAsync();
            }

            public async Task<Payroll?> GetByIdAsync(int payrollId)
            {
                return await _context.Payrolls
                    .FirstOrDefaultAsync(p => p.PayrollId == payrollId);
            }

            public async Task AddAsync(Payroll payroll)
            {
                _context.Payrolls.Add(payroll);
                await _context.SaveChangesAsync();
            }

            public async Task<bool> UpdateAsync(int payrollId, Payroll payroll)
            {
                var existing = await _context.Payrolls.FindAsync(payrollId);
                if (existing == null)
                    return false;

                existing.EmployeeId = payroll.EmployeeId;
                existing.BasicSalary = payroll.BasicSalary;
                existing.Allowances = payroll.Allowances;
                existing.Deductions = payroll.Deductions;
                existing.GrossSalary = payroll.GrossSalary;
                existing.NetSalary = payroll.NetSalary;
                existing.PayrollMonth = payroll.PayrollMonth;

                await _context.SaveChangesAsync();
                return true;
            }

            public async Task<bool> DeleteAsync(int payrollId)
            {
                var payroll = await _context.Payrolls.FindAsync(payrollId);
                if (payroll == null)
                    return false;

                _context.Payrolls.Remove(payroll);
                await _context.SaveChangesAsync();
                return true;
            }
        
    }
}

