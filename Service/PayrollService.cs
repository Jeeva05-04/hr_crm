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

        // Count Mon-Fri working days in a given month
        private int GetWorkingDaysInMonth(int year, int month)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            int workingDays = 0;
            for (int d = 1; d <= daysInMonth; d++)
            {
                var day = new DateTime(year, month, d).DayOfWeek;
                if (day != DayOfWeek.Saturday && day != DayOfWeek.Sunday)
                    workingDays++;
            }
            return workingDays;
        }

        // =============================================
        // Indian New Tax Regime slabs (FY 2024-25)
        // =============================================
        private decimal CalculateMonthlyTax(decimal annualSalary)
        {
            decimal tax = 0;

            if (annualSalary <= 300000)
                tax = 0;
            else if (annualSalary <= 600000)
                tax = (annualSalary - 300000) * 0.05m;
            else if (annualSalary <= 900000)
                tax = 15000 + (annualSalary - 600000) * 0.10m;
            else if (annualSalary <= 1200000)
                tax = 45000 + (annualSalary - 900000) * 0.15m;
            else if (annualSalary <= 1500000)
                tax = 90000 + (annualSalary - 1200000) * 0.20m;
            else
                tax = 150000 + (annualSalary - 1500000) * 0.30m;

            // Add 4% Health & Education cess
            tax = tax * 1.04m;

            return Math.Round(tax / 12, 2); // monthly TDS
        }

        private PayrollResponseDto MapToResponse(Payroll p) => new()
        {
            PayrollId = p.PayrollId,
            UserId = p.UserId,
            UserName = p.UserName,
            Month = p.Month,
            Year = p.Year,

            EmploymentType = p.EmploymentType,
            Department = p.Department,
            Designation = p.Designation,
            DOJ = p.DOJ,

            MonthlyCTC = p.MonthlyCTC,
            NoOfPayableDays = p.NoOfPayableDays,
            MonthlyCTCApportioned = p.MonthlyCTCApportioned,

            BasicSalary = p.BasicSalary,
            HRA = p.HRA,
            ConveyanceAllowance = p.ConveyanceAllowance,
            MedicalAllowance = p.MedicalAllowance,
            OtherAllowance = p.OtherAllowance,
            TAOrPBonus = p.TAOrPBonus,
            GrossSalary = p.GrossSalary,

            EmployeePF = p.EmployeePF,
            PT = p.PT,
            EmployerPF = p.EmployerPF,
            TDS = p.TaxDeduction,

            NetPay = p.NetSalary,

            WorkingDays = p.WorkingDays,
            PresentDays = p.PresentDays,
            Status = p.Status,
            CreatedDate = p.CreatedDate,
            ApprovedBy = p.ApprovedBy,
            ApprovedDate = p.ApprovedDate,
            // keep legacy fields too
            TotalAllowances = p.TotalAllowances,
            AbsentDeduction = p.AbsentDeduction,
            TotalDeductions = p.TotalDeductions,
            BonusAmount = p.BonusAmount,
            OvertimePay = p.OvertimePay
        };

        // =============================================
        // Generate Payroll (with all 4 improvements)
        // =============================================
        public async Task<(PayrollResponseDto? Result, string? Error)> GeneratePayrollAsync(PayrollCreateDto dto)
        {
            var currentMonth = GetCurrentPayrollMonth();
            var year = currentMonth.Year;
            var month = currentMonth.Month;

            // 3. Duplicate prevention
            var existing = await _repo.GetPayrollByUserMonthYearAsync(dto.UserId, currentMonth, year);
            if (existing != null)
                return (null, $"Payroll already generated for this user for {currentMonth:MMMM yyyy}. Current status: {existing.Status}.");

            // Load allowances and deductions
            var allowances = await _repo.GetAllowancesAsync(dto.UserId, currentMonth, year);
            var deductions = await _repo.GetDeductionsAsync(dto.UserId, currentMonth, year);

            decimal totalAllowances = allowances.Sum(a => a.Amount);
            decimal totalDeductions = deductions.Sum(d => d.Amount);

            // 1. Auto overtime pay
            int workingDays = GetWorkingDaysInMonth(year, month);
            double overtimeHours = await _repo.GetOvertimeHoursForMonthAsync(dto.UserId, year, month);

            // NOTE: we will compute amounts based on Monthly CTC (from salary config if present)
            var salaryConfig = await _repo.GetSalaryConfigAsync(dto.UserId);
            // Fallback defaults
            const decimal defaultConveyance = 1600m;
            const decimal defaultMedical = 1250m;

            decimal monthlyCTC = 0m;
            if (salaryConfig != null && salaryConfig.MonthlyCTC > 0)
                monthlyCTC = salaryConfig.MonthlyCTC;
            else if (dto.BasicSalary > 0)
                monthlyCTC = dto.BasicSalary * 2m; // assume Basic = 50% of CTC when no config

            decimal dailySalary = workingDays > 0 ? monthlyCTC / workingDays : 0;
            decimal hourlySalary = dailySalary / 8;
            decimal overtimePay = (decimal)overtimeHours * hourlySalary * 1.5m; // 1.5x rate

            // 2. Attendance-based absent deduction
            int presentDays = await _repo.GetPresentDaysForMonthAsync(dto.UserId, year, month);
            int absentDays = Math.Max(0, workingDays - presentDays);
            decimal absentDeduction = absentDays * dailySalary;

            // Monthly CTC apportioned to paid days
            decimal monthlyCTCApportioned = workingDays > 0 ? Math.Round(monthlyCTC * (decimal)presentDays / workingDays, 2) : 0m;

            // Basic = 50% of apportioned CTC
            decimal basicSalary = Math.Round(monthlyCTCApportioned * 0.50m, 2);
            // HRA = 50% of Basic
            decimal hra = Math.Round(basicSalary * 0.50m, 2);
            // Conveyance & Medical from config or defaults
            decimal conveyance = salaryConfig != null && salaryConfig.Conveyance > 0 ? salaryConfig.Conveyance : defaultConveyance;
            decimal medical = salaryConfig != null && salaryConfig.MedicalAllowance > 0 ? salaryConfig.MedicalAllowance : defaultMedical;

            // Other allowance = apportioned CTC - (Basic + HRA + Conveyance + Medical)
            decimal otherAllowance = Math.Round(monthlyCTCApportioned - (basicSalary + hra + conveyance + medical), 2);
            if (otherAllowance < 0) otherAllowance = 0m;

            // Bonuses/TA

            // 3. Auto-pickup approved bonuses for this month
            var bonuses = await _repo.GetApprovedBonusesForMonthAsync(dto.UserId, month, year);
            decimal bonusAmount = bonuses.Sum(b => b.Amount);

            // 4. Tax calculation (TDS) — based on projected annual salary
            // Tax calculation (projected annual salary based on full monthly CTC)
            decimal annualSalary = monthlyCTC * 12;
            decimal monthlyTax = CalculateMonthlyTax(annualSalary);

            // Gross salary (sum of earnings)
            decimal grossSalary = Math.Round(basicSalary + hra + conveyance + medical + otherAllowance + overtimePay + bonusAmount + totalAllowances, 2);

            // Net salary calculation
            decimal netSalary = grossSalary - totalDeductions - absentDeduction - monthlyTax;

            var payroll = new Payroll
            {
                UserId = dto.UserId,
                UserName = dto.UserName,
                // payroll formula fields
                MonthlyCTC = monthlyCTC,
                NoOfPayableDays = presentDays,
                MonthlyCTCApportioned = monthlyCTCApportioned,
                BasicSalary = basicSalary,
                HRA = hra,
                ConveyanceAllowance = conveyance,
                MedicalAllowance = medical,
                OtherAllowance = otherAllowance,
                OvertimePay = Math.Round(overtimePay, 2),
                TAOrPBonus = Math.Round(bonusAmount, 2),
                BonusAmount = Math.Round(bonusAmount, 2),
                TotalAllowances = totalAllowances,
                GrossSalary = grossSalary,
                AbsentDeduction = Math.Round(absentDeduction, 2),
                TaxDeduction = monthlyTax,
                TotalDeductions = totalDeductions,
                WorkingDays = workingDays,
                PresentDays = presentDays,
                NetSalary = Math.Round(netSalary, 2),
                Month = currentMonth,
                Year = year,
                Status = "Draft",
                CreatedDate = DateTime.UtcNow
            };

            var result = await _repo.GeneratePayrollAsync(payroll);
            return (MapToResponse(result), null);
        }

        public async Task<List<PayrollResponseDto>> GetAllPayrollAsync()
        {
            var payrolls = await _repo.GetAllPayrollAsync();
            return payrolls.Select(MapToResponse).ToList();
        }

        public async Task<List<PayrollResponseDto>> GetPayrollByUserIdAsync(int userId)
        {
            var payrolls = await _repo.GetPayrollByUserIdAsync(userId);
            return payrolls.Select(MapToResponse).ToList();
        }

        public async Task<PayrollResponseDto?> UpdatePayrollAsync(int payrollId, PayrollCreateDto dto)
        {
            var existing = await _repo.GetPayrollByIdAsync(payrollId);
            if (existing == null) return null;

            // Can only edit Draft payrolls
            if (existing.Status != "Draft") return null;

            var year = existing.Month.Year;
            var month = existing.Month.Month;

            var allowances = await _repo.GetAllowancesAsync(dto.UserId, existing.Month, year);
            var deductions = await _repo.GetDeductionsAsync(dto.UserId, existing.Month, year);

            decimal totalAllowances = allowances.Sum(a => a.Amount);
            decimal totalDeductions = deductions.Sum(d => d.Amount);

            int workingDays = GetWorkingDaysInMonth(year, month);
            double overtimeHours = await _repo.GetOvertimeHoursForMonthAsync(dto.UserId, year, month);

            // Use salary configuration if present
            var salaryConfig = await _repo.GetSalaryConfigAsync(dto.UserId);
            const decimal defaultConveyance = 1600m;
            const decimal defaultMedical = 1250m;

            decimal monthlyCTC = 0m;
            if (salaryConfig != null && salaryConfig.MonthlyCTC > 0)
                monthlyCTC = salaryConfig.MonthlyCTC;
            else if (dto.BasicSalary > 0)
                monthlyCTC = dto.BasicSalary * 2m;

            decimal dailySalary = workingDays > 0 ? monthlyCTC / workingDays : 0;
            decimal overtimePay = (decimal)overtimeHours * (dailySalary / 8) * 1.5m;

            int presentDays = await _repo.GetPresentDaysForMonthAsync(dto.UserId, year, month);
            int absentDays = Math.Max(0, workingDays - presentDays);
            decimal absentDeduction = absentDays * dailySalary;

            decimal monthlyCTCApportioned = workingDays > 0 ? Math.Round(monthlyCTC * (decimal)presentDays / workingDays, 2) : 0m;
            decimal basicSalary = Math.Round(monthlyCTCApportioned * 0.50m, 2);
            decimal hra = Math.Round(basicSalary * 0.50m, 2);
            decimal conveyance = salaryConfig != null && salaryConfig.Conveyance > 0 ? salaryConfig.Conveyance : defaultConveyance;
            decimal medical = salaryConfig != null && salaryConfig.MedicalAllowance > 0 ? salaryConfig.MedicalAllowance : defaultMedical;
            decimal otherAllowance = Math.Round(monthlyCTCApportioned - (basicSalary + hra + conveyance + medical), 2);
            if (otherAllowance < 0) otherAllowance = 0m;

            var bonuses = await _repo.GetApprovedBonusesForMonthAsync(dto.UserId, month, year);
            decimal bonusAmount = bonuses.Sum(b => b.Amount);

            decimal annualSalary = monthlyCTC * 12;
            decimal monthlyTax = CalculateMonthlyTax(annualSalary);

            decimal grossSalary = Math.Round(basicSalary + hra + conveyance + medical + otherAllowance + overtimePay + bonusAmount + totalAllowances, 2);
            decimal netSalary = grossSalary - totalDeductions - absentDeduction - monthlyTax;

            existing.UserId = dto.UserId;
            existing.UserName = dto.UserName;
            existing.MonthlyCTC = monthlyCTC;
            existing.MonthlyCTCApportioned = monthlyCTCApportioned;
            existing.NoOfPayableDays = presentDays;
            existing.BasicSalary = basicSalary;
            existing.HRA = hra;
            existing.ConveyanceAllowance = conveyance;
            existing.MedicalAllowance = medical;
            existing.OtherAllowance = otherAllowance;
            existing.OvertimePay = Math.Round(overtimePay, 2);
            existing.TAOrPBonus = Math.Round(bonusAmount, 2);
            existing.BonusAmount = Math.Round(bonusAmount, 2);
            existing.TotalAllowances = totalAllowances;
            existing.GrossSalary = grossSalary;
            existing.AbsentDeduction = Math.Round(absentDeduction, 2);
            existing.TaxDeduction = monthlyTax;
            existing.TotalDeductions = totalDeductions;
            existing.WorkingDays = workingDays;
            existing.PresentDays = presentDays;
            existing.NetSalary = Math.Round(netSalary, 2);

            var updated = await _repo.UpdatePayrollAsync(existing);
            return updated == null ? null : MapToResponse(updated);
        }

        public async Task<bool> DeletePayrollAsync(int payrollId)
        {
            return await _repo.DeletePayrollAsync(payrollId);
        }

        public async Task AddAllowanceAsync(AllowanceCreateDto dto)
        {
            var currentMonth = GetCurrentPayrollMonth();
            await _repo.AddAllowanceAsync(new Allowance
            {
                UserId = dto.UserId,
                AllowanceType = dto.AllowanceType,
                Amount = dto.Amount,
                Month = currentMonth,
                Year = currentMonth.Year
            });
        }

        public async Task AddDeductionAsync(DeductionCreateDto dto)
        {
            var currentMonth = GetCurrentPayrollMonth();
            await _repo.AddDeductionAsync(new Deduction
            {
                UserId = dto.UserId,
                DeductionType = dto.DeductionType,
                Amount = dto.Amount,
                Month = currentMonth,
                Year = currentMonth.Year
            });
        }

        // =============================================
        // Payslip — itemized breakdown
        // =============================================
        public async Task<PayslipDto?> GetCurrentPayslipAsync(int userId)
        {
            var currentMonth = GetCurrentPayrollMonth();
            var year = currentMonth.Year;

            var payroll = await _repo.GetPayrollByUserMonthYearAsync(userId, currentMonth, year);
            if (payroll == null) return null;

            var allowances = await _repo.GetAllowancesAsync(userId, currentMonth, year);
            var deductions = await _repo.GetDeductionsAsync(userId, currentMonth, year);

            var bonuses = await _repo.GetApprovedBonusesForMonthAsync(userId, currentMonth.Month, year);
            string bonusDetails = bonuses.Any()
                ? string.Join(" + ", bonuses.Select(b => b.BonusType))
                : string.Empty;

            decimal annualSalary = (payroll.BasicSalary + payroll.TotalAllowances) * 12;

            return new PayslipDto
            {
                UserId = payroll.UserId,
                UserName = payroll.UserName,
                Month = payroll.Month,
                Year = payroll.Year,
                WorkingDays = payroll.WorkingDays,
                PresentDays = payroll.PresentDays,
                BasicSalary = payroll.BasicSalary,
                OvertimePay = payroll.OvertimePay,
                BonusAmount = payroll.BonusAmount,
                BonusDetails = bonusDetails,
                Allowances = allowances.Select(a => new AllowanceItemDto
                {
                    Type = a.AllowanceType,
                    Amount = a.Amount
                }).ToList(),
                TotalAllowances = payroll.TotalAllowances,
                AbsentDeduction = payroll.AbsentDeduction,
                TaxDeduction = payroll.TaxDeduction,
                AnnualSalary = annualSalary,
                Deductions = deductions.Select(d => new DeductionItemDto
                {
                    Type = d.DeductionType,
                    Amount = d.Amount
                }).ToList(),
                TotalDeductions = payroll.TotalDeductions,
                NetSalary = payroll.NetSalary,
                Status = payroll.Status,
                GeneratedDate = payroll.CreatedDate
            };
        }

        // =============================================
        // 4. Approval Workflow
        // =============================================
        public async Task<(bool Success, string? Error)> ApprovePayrollAsync(int payrollId, int approvedBy)
        {
            var payroll = await _repo.GetPayrollByIdAsync(payrollId);
            if (payroll == null)
                return (false, "Payroll record not found.");

            if (payroll.Status != "Draft")
                return (false, $"Cannot approve. Payroll is already '{payroll.Status}'.");

            var result = await _repo.ApprovePayrollAsync(payrollId, approvedBy);
            return result ? (true, null) : (false, "Approval failed.");
        }

        public async Task<(bool Success, string? Error)> MarkAsPaidAsync(int payrollId)
        {
            var payroll = await _repo.GetPayrollByIdAsync(payrollId);
            if (payroll == null)
                return (false, "Payroll record not found.");

            if (payroll.Status != "Approved")
                return (false, $"Cannot mark as paid. Payroll must be 'Approved' first. Current status: '{payroll.Status}'.");

            var result = await _repo.MarkAsPaidAsync(payrollId);
            return result ? (true, null) : (false, "Failed to mark as paid.");
        }

        // =============================================
        // Bonus
        // =============================================
        public async Task<(Bonus? Result, string? Error)> CreateBonusAsync(BonusCreateDto dto, int createdBy)
        {
            if (dto.Amount <= 0)
                return (null, "Bonus amount must be greater than zero.");

            var bonus = new Bonus
            {
                UserId = dto.UserId,
                UserName = dto.UserName,
                BonusType = dto.BonusType,
                Amount = dto.Amount,
                Reason = dto.Reason,
                Month = dto.Month,
                Year = dto.Year,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            var result = await _repo.CreateBonusAsync(bonus);
            return (result, null);
        }

        public async Task<List<Bonus>> GetBonusesByUserAsync(int userId)
            => await _repo.GetBonusesByUserAsync(userId);

        public async Task<List<Bonus>> GetAllBonusesAsync()
            => await _repo.GetAllBonusesAsync();

        public async Task<(bool Success, string? Error)> ApproveBonusAsync(int id, int approvedBy)
        {
            var bonus = await _repo.GetBonusByIdAsync(id);
            if (bonus == null)
                return (false, "Bonus not found.");
            if (bonus.Status != "Pending")
                return (false, $"Bonus is already '{bonus.Status}'.");

            var result = await _repo.ApproveBonusAsync(id, approvedBy);
            return result ? (true, null) : (false, "Approval failed.");
        }

        // =============================================
        // Salary Configuration
        // =============================================
        public async Task<SalaryConfiguration> SetSalaryConfigAsync(SalaryConfigDto dto, int setBy)
        {
            var config = new SalaryConfiguration
            {
                UserId = dto.UserId,
                UserName = dto.UserName,
                BasicSalary = dto.BasicSalary,
                MonthlyCTC = dto.MonthlyCTC,
                Conveyance = dto.Conveyance,
                MedicalAllowance = dto.MedicalAllowance,
                EffectiveFrom = GetCurrentPayrollMonth(),
                CreatedDate = DateTime.UtcNow,
                SetBy = setBy
            };
            return await _repo.SetSalaryConfigAsync(config);
        }

        public async Task<SalaryConfiguration?> GetSalaryConfigAsync(int userId)
            => await _repo.GetSalaryConfigAsync(userId);

        public async Task<List<SalaryConfiguration>> GetAllSalaryConfigsAsync()
            => await _repo.GetAllSalaryConfigsAsync();

        // =============================================
        // Auto Payroll Generation (runs on 1st of every month)
        // =============================================
        public async Task<(int Generated, int Skipped)> AutoGeneratePayrollForAllAsync()
        {
            var configs = await _repo.GetAllSalaryConfigsAsync();
            int generated = 0;
            int skipped = 0;

            var currentMonth = GetCurrentPayrollMonth();
            var year = currentMonth.Year;
            var month = currentMonth.Month;
            int workingDays = GetWorkingDaysInMonth(year, month);

            foreach (var config in configs)
            {
                // Skip if payroll already generated for this user this month
                var existing = await _repo.GetPayrollByUserMonthYearAsync(config.UserId, currentMonth, year);
                if (existing != null)
                {
                    skipped++;
                    continue;
                }

                var allowances = await _repo.GetAllowancesAsync(config.UserId, currentMonth, year);
                var deductions = await _repo.GetDeductionsAsync(config.UserId, currentMonth, year);

                decimal totalAllowances = allowances.Sum(a => a.Amount);
                decimal totalDeductions = deductions.Sum(d => d.Amount);

                double overtimeHours = await _repo.GetOvertimeHoursForMonthAsync(config.UserId, year, month);
                decimal dailySalary = workingDays > 0 ? config.BasicSalary / workingDays : 0;
                decimal overtimePay = (decimal)overtimeHours * (dailySalary / 8) * 1.5m;

                int presentDays = await _repo.GetPresentDaysForMonthAsync(config.UserId, year, month);
                int absentDays = Math.Max(0, workingDays - presentDays);
                decimal absentDeduction = absentDays * dailySalary;

                var bonuses = await _repo.GetApprovedBonusesForMonthAsync(config.UserId, month, year);
                decimal bonusAmount = bonuses.Sum(b => b.Amount);

                decimal annualSalary = (config.BasicSalary + totalAllowances) * 12;
                decimal monthlyTax = CalculateMonthlyTax(annualSalary);

                decimal netSalary = config.BasicSalary + overtimePay + bonusAmount + totalAllowances
                                    - totalDeductions - absentDeduction - monthlyTax;

                var payroll = new Payroll
                {
                    UserId = config.UserId,
                    UserName = config.UserName,
                    BasicSalary = config.BasicSalary,
                    OvertimePay = Math.Round(overtimePay, 2),
                    BonusAmount = Math.Round(bonusAmount, 2),
                    TotalAllowances = totalAllowances,
                    AbsentDeduction = Math.Round(absentDeduction, 2),
                    TaxDeduction = monthlyTax,
                    TotalDeductions = totalDeductions,
                    WorkingDays = workingDays,
                    PresentDays = presentDays,
                    NetSalary = Math.Round(netSalary, 2),
                    Month = currentMonth,
                    Year = year,
                    Status = "Draft",
                    CreatedDate = DateTime.UtcNow
                };

                await _repo.GeneratePayrollAsync(payroll);
                generated++;
            }

            return (generated, skipped);
        }
    }
}
