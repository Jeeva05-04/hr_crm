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

        public async Task<Payroll> GeneratePayrollAsync(Payroll payroll)
        {
            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();
            return payroll;
        }

        public async Task<List<Payroll>> GetAllPayrollAsync()
        {
            return await _context.Payrolls.ToListAsync();
        }

        public async Task<List<Payroll>> GetPayrollByUserIdAsync(int userId)
        {
            return await _context.Payrolls
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<Payroll?> GetPayrollByIdAsync(int payrollId)
        {
            return await _context.Payrolls.FirstOrDefaultAsync(p => p.PayrollId == payrollId);
        }

        public async Task<Payroll?> UpdatePayrollAsync(Payroll payroll)
        {
            _context.Payrolls.Update(payroll);
            await _context.SaveChangesAsync();
            return payroll;
        }
        public async Task<bool> DeletePayrollAsync(int payrollId)
        {
            var payroll = await _context.Payrolls.FirstOrDefaultAsync(p => p.PayrollId == payrollId);

            if (payroll == null)
                return false;

            _context.Payrolls.Remove(payroll);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AddAllowanceAsync(Allowance allowance)
        {
            _context.Allowances.Add(allowance);
            await _context.SaveChangesAsync();
        }

        public async Task AddDeductionAsync(Deduction deduction)
        {
            _context.Deductions.Add(deduction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Allowance>> GetAllowancesAsync(int userId, DateTime month, int year)
        {
            return await _context.Allowances
                .Where(a => a.UserId == userId && a.Month == month && a.Year == year)
                .ToListAsync();
        }

        public async Task<List<Deduction>> GetDeductionsAsync(int userId, DateTime month, int year)
        {
            return await _context.Deductions
                .Where(d => d.UserId == userId && d.Month == month && d.Year == year)
                .ToListAsync();
        }
        public async Task<Payroll?> GetPayrollByUserMonthYearAsync(int userId, DateTime month, int year)
        {
            return await _context.Payrolls
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Month == month && p.Year == year);
        }
    }
}

