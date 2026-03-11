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

        private DateTime GetCurrentPayrollMonth()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public async Task<PayrollResponseDto> GeneratePayrollAsync(PayrollCreateDto dto)
        {
            var currentMonth = GetCurrentPayrollMonth();
            var currentYear = currentMonth.Year;

            var allowances = await _repo.GetAllowancesAsync(dto.UserId, currentMonth, currentYear);
            var deductions = await _repo.GetDeductionsAsync(dto.UserId, currentMonth, currentYear);

            decimal totalAllowances = allowances.Sum(a => a.Amount);
            decimal totalDeductions = deductions.Sum(d => d.Amount);
            decimal netSalary = dto.BasicSalary + totalAllowances - totalDeductions;

            var payroll = new Payroll
            {
                UserId = dto.UserId,
                UserName = dto.UserName,
                BasicSalary = dto.BasicSalary,
                TotalAllowances = totalAllowances,
                TotalDeductions = totalDeductions,
                NetSalary = netSalary,
                Month = currentMonth,
                Year = currentYear,
                Status = dto.Status,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _repo.GeneratePayrollAsync(payroll);

            return new PayrollResponseDto
            {
                PayrollId = result.PayrollId,
                UserId = result.UserId,
                BasicSalary = result.BasicSalary,
                TotalAllowances = result.TotalAllowances,
                TotalDeductions = result.TotalDeductions,
                NetSalary = result.NetSalary,
                Month = result.Month,
                Year = result.Year,
                Status = result.Status,
                CreatedDate = result.CreatedDate
            };
        }

        public async Task<List<PayrollResponseDto>> GetAllPayrollAsync()
        {
            var payrolls = await _repo.GetAllPayrollAsync();

            return payrolls.Select(p => new PayrollResponseDto
            {
                PayrollId = p.PayrollId,
                UserId = p.UserId,
                BasicSalary = p.BasicSalary,
                TotalAllowances = p.TotalAllowances,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                Month = p.Month,
                Year = p.Year,
                Status = p.Status,
                CreatedDate = p.CreatedDate
            }).ToList();
        }

        public async Task<List<PayrollResponseDto>> GetPayrollByUserIdAsync(int userId)
        {
            var payrolls = await _repo.GetPayrollByUserIdAsync(userId);

            return payrolls.Select(p => new PayrollResponseDto
            {
                PayrollId = p.PayrollId,
                UserId = p.UserId,
                BasicSalary = p.BasicSalary,
                TotalAllowances = p.TotalAllowances,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                Month = p.Month,
                Year = p.Year,
                Status = p.Status,
                CreatedDate = p.CreatedDate
            }).ToList();
        }

        public async Task<PayrollResponseDto?> UpdatePayrollAsync(int payrollId, PayrollCreateDto dto)
        {
            var existing = await _repo.GetPayrollByIdAsync(payrollId);
            if (existing == null)
                return null;

            var currentMonth = GetCurrentPayrollMonth();
            var currentYear = currentMonth.Year;

            var allowances = await _repo.GetAllowancesAsync(dto.UserId, currentMonth, currentYear);
            var deductions = await _repo.GetDeductionsAsync(dto.UserId, currentMonth, currentYear);

            decimal totalAllowances = allowances.Sum(a => a.Amount);
            decimal totalDeductions = deductions.Sum(d => d.Amount);
            decimal netSalary = dto.BasicSalary + totalAllowances - totalDeductions;

            existing.UserId = dto.UserId;
            existing.UserName = dto.UserName;
            existing.BasicSalary = dto.BasicSalary;
            existing.TotalAllowances = totalAllowances;
            existing.TotalDeductions = totalDeductions;
            existing.NetSalary = netSalary;
            existing.Month = currentMonth;
            existing.Year = currentYear;
            existing.Status = dto.Status;

            var updated = await _repo.UpdatePayrollAsync(existing);
            if (updated == null)
                return null;

            return new PayrollResponseDto
            {
                PayrollId = updated.PayrollId,
                UserId = updated.UserId,
                BasicSalary = updated.BasicSalary,
                TotalAllowances = updated.TotalAllowances,
                TotalDeductions = updated.TotalDeductions,
                NetSalary = updated.NetSalary,
                Month = updated.Month,
                Year = updated.Year,
                Status = updated.Status,
                CreatedDate = updated.CreatedDate
            };
        }

        public async Task<bool> DeletePayrollAsync(int payrollId)
        {
            return await _repo.DeletePayrollAsync(payrollId);
        }

        public async Task AddAllowanceAsync(AllowanceCreateDto dto)
        {
            var currentMonth = GetCurrentPayrollMonth();
            var currentYear = currentMonth.Year;

            var allowance = new Allowance
            {
                UserId = dto.UserId,
                AllowanceType = dto.AllowanceType,
                Amount = dto.Amount,
                Month = currentMonth,
                Year = currentYear
            };

            await _repo.AddAllowanceAsync(allowance);
        }

        public async Task AddDeductionAsync(DeductionCreateDto dto)
        {
            var currentMonth = GetCurrentPayrollMonth();
            var currentYear = currentMonth.Year;

            var deduction = new Deduction
            {
                UserId = dto.UserId,
                DeductionType = dto.DeductionType,
                Amount = dto.Amount,
                Month = currentMonth,
                Year = currentYear
            };

            await _repo.AddDeductionAsync(deduction);
        }

        public async Task<PayslipDto?> GetCurrentPayslipAsync(int userId)
        {
            var currentMonth = GetCurrentPayrollMonth();
            var currentYear = currentMonth.Year;

            var payroll = await _repo.GetPayrollByUserMonthYearAsync(userId, currentMonth, currentYear);

            if (payroll == null)
                return null;

            return new PayslipDto
            {
                UserId = payroll.UserId,
                UserName = payroll.UserName,
                Month = payroll.Month,
                Year = payroll.Year,
                BasicSalary = payroll.BasicSalary,
                TotalAllowances = payroll.TotalAllowances,
                TotalDeductions = payroll.TotalDeductions,
                NetSalary = payroll.NetSalary,
                GeneratedDate = DateTime.UtcNow
            };
        }
    }
}