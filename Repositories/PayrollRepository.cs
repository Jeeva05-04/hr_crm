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
            return await _context.Payrolls.OrderByDescending(p => p.Month).ToListAsync();
        }

        public async Task<List<Payroll>> GetPayrollByUserIdAsync(int userId)
        {
            return await _context.Payrolls
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Month)
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
            if (payroll == null) return false;
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
                .Where(a => a.UserId == userId && a.Month.Month == month.Month && a.Year == year)
                .ToListAsync();
        }

        public async Task<List<Deduction>> GetDeductionsAsync(int userId, DateTime month, int year)
        {
            return await _context.Deductions
                .Where(d => d.UserId == userId && d.Month.Month == month.Month && d.Year == year)
                .ToListAsync();
        }

        public async Task<Payroll?> GetPayrollByUserMonthYearAsync(int userId, DateTime month, int year)
        {
            return await _context.Payrolls
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.Month.Month == month.Month &&
                    p.Year == year);
        }

        // =============================================
        // Overtime Integration
        // =============================================
        public async Task<double> GetOvertimeHoursForMonthAsync(int userId, int year, int month)
        {
            return await _context.OvertimeRecords
                .Where(o => o.UserId == userId &&
                            o.Date.Year == year &&
                            o.Date.Month == month)
                .SumAsync(o => o.OvertimeHours);
        }

        // =============================================
        // Attendance Integration
        // =============================================
        public async Task<int> GetPresentDaysForMonthAsync(int userId, int year, int month)
        {
            return await _context.Attendances
                .Where(a => a.UserId == userId &&
                            a.AttendanceDate.Year == year &&
                            a.AttendanceDate.Month == month &&
                            a.Status == "Present")
                .Select(a => a.AttendanceDate.Date)
                .Distinct()
                .CountAsync();
        }

        // =============================================
        // Approval Workflow
        // =============================================
        public async Task<bool> ApprovePayrollAsync(int payrollId, int approvedBy)
        {
            var payroll = await _context.Payrolls.FirstOrDefaultAsync(p => p.PayrollId == payrollId);
            if (payroll == null) return false;
            if (payroll.Status != "Draft") return false;

            payroll.Status = "Approved";
            payroll.ApprovedBy = approvedBy;
            payroll.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsPaidAsync(int payrollId)
        {
            var payroll = await _context.Payrolls.FirstOrDefaultAsync(p => p.PayrollId == payrollId);
            if (payroll == null) return false;
            if (payroll.Status != "Approved") return false;

            payroll.Status = "Paid";
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // Bonus
        // =============================================
        public async Task<Bonus> CreateBonusAsync(Bonus bonus)
        {
            _context.Bonuses.Add(bonus);
            await _context.SaveChangesAsync();
            return bonus;
        }

        public async Task<List<Bonus>> GetBonusesByUserAsync(int userId)
        {
            return await _context.Bonuses
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Year).ThenByDescending(b => b.Month)
                .ToListAsync();
        }

        public async Task<List<Bonus>> GetAllBonusesAsync()
        {
            return await _context.Bonuses
                .OrderByDescending(b => b.Year).ThenByDescending(b => b.Month)
                .ToListAsync();
        }

        public async Task<Bonus?> GetBonusByIdAsync(int id)
        {
            return await _context.Bonuses.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> ApproveBonusAsync(int id, int approvedBy)
        {
            var bonus = await _context.Bonuses.FirstOrDefaultAsync(b => b.Id == id);
            if (bonus == null || bonus.Status != "Pending") return false;

            bonus.Status = "Approved";
            bonus.ApprovedBy = approvedBy;
            bonus.ApprovedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Bonus>> GetApprovedBonusesForMonthAsync(int userId, int month, int year)
        {
            return await _context.Bonuses
                .Where(b => b.UserId == userId && b.Month == month && b.Year == year && b.Status == "Approved")
                .ToListAsync();
        }

        // =============================================
        // Salary Configuration
        // =============================================
        public async Task<SalaryConfiguration> SetSalaryConfigAsync(SalaryConfiguration config)
        {
            var existing = await _context.SalaryConfigurations
                .FirstOrDefaultAsync(s => s.UserId == config.UserId);

            if (existing == null)
            {
                _context.SalaryConfigurations.Add(config);
            }
            else
            {
                existing.BasicSalary = config.BasicSalary;
                existing.UserName = config.UserName;
                existing.UpdatedDate = DateTime.UtcNow;
                existing.SetBy = config.SetBy;
                config = existing;
            }

            await _context.SaveChangesAsync();
            return config;
        }

        public async Task<SalaryConfiguration?> GetSalaryConfigAsync(int userId)
        {
            return await _context.SalaryConfigurations
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<List<SalaryConfiguration>> GetAllSalaryConfigsAsync()
        {
            return await _context.SalaryConfigurations
                .OrderBy(s => s.UserId)
                .ToListAsync();
        }
    }
}
