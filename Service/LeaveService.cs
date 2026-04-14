using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Service
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _repo;
        private readonly IPayrollRepository _payrollRepo;

        public LeaveService(ILeaveRepository repo, IPayrollRepository payrollRepo)
        {
            _repo = repo;
            _payrollRepo = payrollRepo;
        }

        // Calculate actual leave days excluding weekends and holidays
        private async Task<int> CalculateLeaveDaysAsync(DateTime start, DateTime end)
        {
            var holidayDates = await _repo.GetHolidayDatesAsync(start.Year);
            int days = 0;
            for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Saturday &&
                    d.DayOfWeek != DayOfWeek.Sunday &&
                    !holidayDates.Contains(d))
                {
                    days++;
                }
            }
            return days;
        }

        // =============================================
        // Apply Leave
        // =============================================
        public async Task<(bool Success, string? Error)> ApplyLeaveAsync(LeaveCreateDto dto)
        {
            var year = dto.StartDate.Year;

            await _repo.InitBalanceAsync(dto.UserId, year);

            int totalDays = await CalculateLeaveDaysAsync(dto.StartDate, dto.EndDate);

            if (totalDays <= 0)
                return (false, "No working days in the selected date range (weekends/holidays excluded).");

            var balance = await _repo.GetBalanceAsync(dto.UserId, dto.LeaveType, year);
            if (balance == null)
                return (false, $"Leave type '{dto.LeaveType}' is not valid. Use Sick, Casual, or Earned.");

            if (balance.RemainingDays < totalDays)
                return (false, $"Insufficient {dto.LeaveType} leave balance. Available: {balance.RemainingDays} days, Requested: {totalDays} days.");

            var leave = new Leave
            {
                UserId = dto.UserId,
                LeaveType = dto.LeaveType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = "Pending",
                AppliedOn = DateTime.UtcNow,
                TotalDays = totalDays
            };

            await _repo.AddAsync(leave);
            return (true, null);
        }

        public async Task<List<LeaveResponseDto>> GetAllLeavesAsync()
            => (await _repo.GetAllAsync()).Select(MapToDto).ToList();

        public async Task<List<LeaveResponseDto>> GetLeavesByUserAsync(int userId)
            => (await _repo.GetByUserIdAsync(userId)).Select(MapToDto).ToList();

        // =============================================
        // Update Leave Status
        // =============================================
        public async Task<(bool Success, string? Error)> UpdateLeaveStatusAsync(int leaveId, LeaveStatusDto dto)
        {
            var leave = await _repo.GetByIdAsync(leaveId);
            if (leave == null) return (false, "Leave not found.");

            var previousStatus = leave.Status;
            var newStatus = dto.Status;

            // Approve → deduct balance
            if (newStatus == "Approved" && previousStatus == "Pending")
            {
                var deducted = await _repo.DeductBalanceAsync(leave.UserId, leave.LeaveType, leave.TotalDays, leave.StartDate.Year);
                if (!deducted)
                    return (false, "Insufficient leave balance to approve.");
            }

            // Reject/Cancel after approval → restore balance
            if ((newStatus == "Rejected" || newStatus == "Cancelled") && previousStatus == "Approved")
                await _repo.RestoreBalanceAsync(leave.UserId, leave.LeaveType, leave.TotalDays, leave.StartDate.Year);

            await _repo.UpdateStatusAsync(leaveId, newStatus, dto.ApprovedBY);
            return (true, null);
        }

        public async Task<bool> DeleteLeaveAsync(int leaveId)
            => await _repo.DeleteAsync(leaveId);

        // =============================================
        // Leave Balance
        // =============================================
        public async Task<List<LeaveBalance>> GetBalanceAsync(int userId)
        {
            var year = DateTime.UtcNow.Year;
            await _repo.InitBalanceAsync(userId, year);
            return await _repo.GetBalanceByUserAsync(userId, year);
        }

        // =============================================
        // Holiday Master
        // =============================================
        public async Task<Holiday> AddHolidayAsync(HolidayCreateDto dto, int createdBy)
        {
            var holiday = new Holiday
            {
                Name = dto.Name,
                Date = DateTime.SpecifyKind(dto.Date.Date, DateTimeKind.Utc),
                Type = dto.Type,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            };
            return await _repo.AddHolidayAsync(holiday);
        }

        public async Task<List<Holiday>> GetHolidaysAsync(int year)
            => await _repo.GetHolidaysAsync(year);

        public async Task<bool> DeleteHolidayAsync(int id)
            => await _repo.DeleteHolidayAsync(id);

        // =============================================
        // Leave Calendar
        // =============================================
        public async Task<object> GetCalendarAsync(int month, int year)
        {
            var leaves = await _repo.GetCalendarAsync(month, year);
            var holidays = await _repo.GetHolidaysAsync(year);
            var monthHolidays = holidays.Where(h => h.Date.Month == month).ToList();

            return new
            {
                Month = month,
                Year = year,
                Holidays = monthHolidays.Select(h => new
                {
                    h.Id, h.Name,
                    Date = h.Date.ToString("yyyy-MM-dd"),
                    h.Type
                }),
                Leaves = leaves.Select(l => new
                {
                    l.LeaveId, l.UserId, l.LeaveType,
                    StartDate = l.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = l.EndDate.ToString("yyyy-MM-dd"),
                    l.TotalDays, l.Status
                })
            };
        }

        // =============================================
        // Leave Encashment
        // =============================================
        public async Task<(LeaveEncashment? Result, string? Error)> ProcessEncashmentAsync(
            int userId, string userName, int year, int processedBy)
        {
            await _repo.InitBalanceAsync(userId, year);

            var earnedBalance = await _repo.GetBalanceAsync(userId, "Earned", year);
            if (earnedBalance == null || earnedBalance.RemainingDays <= 0)
                return (null, "No unused Earned leaves available for encashment.");

            var salaryConfig = await _payrollRepo.GetSalaryConfigAsync(userId);
            if (salaryConfig == null)
                return (null, "Salary configuration not found. Please set up salary first.");

            int workingDaysInYear = 260;
            decimal perDaySalary = salaryConfig.BasicSalary * 12 / workingDaysInYear;
            int daysToEncash = earnedBalance.RemainingDays;
            decimal amount = Math.Round(perDaySalary * daysToEncash, 2);

            await _repo.DeductBalanceAsync(userId, "Earned", daysToEncash, year);

            var encashment = new LeaveEncashment
            {
                UserId = userId,
                UserName = userName,
                EncashedDays = daysToEncash,
                AmountPaid = amount,
                Year = year,
                ProcessedDate = DateTime.UtcNow,
                ProcessedBy = processedBy
            };

            var result = await _repo.AddEncashmentAsync(encashment);
            return (result, null);
        }

        public async Task<List<LeaveEncashment>> GetEncashmentsAsync(int userId)
            => await _repo.GetEncashmentsAsync(userId);

        private LeaveResponseDto MapToDto(Leave l) => new()
        {
            LeaveId = l.LeaveId,
            UserId = l.UserId,
            LeaveType = l.LeaveType,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Reason = l.Reason,
            Status = l.Status,
            ApprovedBY = l.ApprovedBY,
            AppliedOn = l.AppliedOn
        };
    }
}
