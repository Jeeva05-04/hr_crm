using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly AppDbContext _context;

        // Default annual allowances per leave type
        private static readonly Dictionary<string, int> DefaultAllowances = new()
        {
            { "Sick",   10 },
            { "Casual", 12 },
            { "Earned", 15 }
        };

        public LeaveRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Leave>> GetAllAsync()
            => await _context.Leaves.OrderByDescending(l => l.AppliedOn).ToListAsync();

        public async Task<List<Leave>> GetByUserIdAsync(int userId)
            => await _context.Leaves.Where(l => l.UserId == userId)
                .OrderByDescending(l => l.AppliedOn).ToListAsync();

        public async Task<Leave?> GetByIdAsync(int leaveId)
            => await _context.Leaves.FirstOrDefaultAsync(l => l.LeaveId == leaveId);

        public async Task AddAsync(Leave leave)
        {
            _context.Leaves.Add(leave);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(int leaveId, string status, string approvedby)
        {
            var leave = await _context.Leaves.FindAsync(leaveId);
            if (leave == null) return false;
            leave.Status = status;
            leave.ApprovedBY = approvedby;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int leaveId)
        {
            var leave = await _context.Leaves.FindAsync(leaveId);
            if (leave == null) return false;
            _context.Leaves.Remove(leave);
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // Leave Balance
        // =============================================
        public async Task<List<LeaveBalance>> GetBalanceByUserAsync(int userId, int year)
            => await _context.LeaveBalances
                .Where(b => b.UserId == userId && b.Year == year)
                .ToListAsync();

        public async Task<LeaveBalance?> GetBalanceAsync(int userId, string leaveType, int year)
            => await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.UserId == userId && b.LeaveType == leaveType && b.Year == year);

        // Initialize leave balance for a new year (called when employee first applies)
        public async Task InitBalanceAsync(int userId, int year)
        {
            foreach (var kv in DefaultAllowances)
            {
                var exists = await _context.LeaveBalances
                    .AnyAsync(b => b.UserId == userId && b.LeaveType == kv.Key && b.Year == year);

                if (!exists)
                {
                    _context.LeaveBalances.Add(new LeaveBalance
                    {
                        UserId = userId,
                        LeaveType = kv.Key,
                        TotalAllowed = kv.Value,
                        UsedDays = 0,
                        Year = year
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeductBalanceAsync(int userId, string leaveType, int days, int year)
        {
            var balance = await GetBalanceAsync(userId, leaveType, year);
            if (balance == null) return false;
            if (balance.RemainingDays < days) return false;

            balance.UsedDays += days;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreBalanceAsync(int userId, string leaveType, int days, int year)
        {
            var balance = await GetBalanceAsync(userId, leaveType, year);
            if (balance == null) return false;

            balance.UsedDays = Math.Max(0, balance.UsedDays - days);
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // Holiday Master
        // =============================================
        public async Task<Holiday> AddHolidayAsync(Holiday holiday)
        {
            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();
            return holiday;
        }

        public async Task<List<Holiday>> GetHolidaysAsync(int year)
            => await _context.Holidays
                .Where(h => h.Date.Year == year)
                .OrderBy(h => h.Date)
                .ToListAsync();

        public async Task<bool> DeleteHolidayAsync(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null) return false;
            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DateTime>> GetHolidayDatesAsync(int year)
            => await _context.Holidays
                .Where(h => h.Date.Year == year)
                .Select(h => h.Date.Date)
                .ToListAsync();

        // =============================================
        // Leave Calendar
        // =============================================
        public async Task<List<Leave>> GetCalendarAsync(int month, int year)
            => await _context.Leaves
                .Where(l => l.Status == "Approved" &&
                            l.StartDate.Month <= month && l.StartDate.Year <= year &&
                            l.EndDate.Month >= month && l.EndDate.Year >= year)
                .OrderBy(l => l.StartDate)
                .ToListAsync();

        // =============================================
        // Leave Encashment
        // =============================================
        public async Task<LeaveEncashment> AddEncashmentAsync(LeaveEncashment encashment)
        {
            _context.LeaveEncashments.Add(encashment);
            await _context.SaveChangesAsync();
            return encashment;
        }

        public async Task<List<LeaveEncashment>> GetEncashmentsAsync(int userId)
            => await _context.LeaveEncashments
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Year)
                .ToListAsync();
    }
}
