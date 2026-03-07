using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AppDbContext _context;

        public AttendanceRepository(AppDbContext context)
        {
            _context = context;
        }

        // =========================================
        // ✅ Check-In (STRICT SHIFT VALIDATION)
        // =========================================
        public async Task<bool> CheckInAsync(int userId, string? ipAddress, double? latitude, double? longitude, string? deviceInfo)
        {
            var today = DateTime.UtcNow.Date;

            var userShift = await _context.UserShifts
                .Include(us => us.Shift)
                .FirstOrDefaultAsync(us => us.UserId == userId);

            if (userShift == null)
                return false;

            var now = DateTime.UtcNow.TimeOfDay;
            var shiftStart = userShift.Shift.StartTime;
            var shiftEnd = userShift.Shift.EndTime;

            bool insideShift;

            if (shiftStart < shiftEnd)
                insideShift = now >= shiftStart && now <= shiftEnd;
            else
                insideShift = now >= shiftStart || now <= shiftEnd;

            if (!insideShift)
                return false;

            var openSession = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.AttendanceDate == today &&
                    a.CheckOutTime == null);

            if (openSession != null)
                return false;

            var newSession = new Attendance
            {
                UserId = userId,
                AttendanceDate = today,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = null,
                Status = "Present",

                // 🌐 New Tracking Fields
                IPAddress = ipAddress,
                Latitude = latitude,
                Longitude = longitude,
                DeviceInfo = deviceInfo
            };

            _context.Attendances.Add(newSession);
            await _context.SaveChangesAsync();

            return true;
        }

        // =========================================
        // ✅ Check-Out (Close Active Session + Overtime Engine)
        // =========================================
        public async Task<bool> CheckOutAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;

            var openSession = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.AttendanceDate == today &&
                    a.CheckOutTime == null);

            if (openSession == null)
                return false;

            openSession.CheckOutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var todaySessions = await _context.Attendances
                .Where(a => a.UserId == userId &&
                            a.AttendanceDate == today &&
                            a.CheckOutTime != null)
                .ToListAsync();

            double totalWorkedHours = todaySessions
                .Sum(s => (s.CheckOutTime.Value - s.CheckInTime).TotalHours);

            var userShift = await _context.UserShifts
                .Include(us => us.Shift)
                .FirstOrDefaultAsync(us => us.UserId == userId);

            if (userShift == null)
                return true;

            var departmentId = userShift.Shift.DepartmentId;

            var policy = await _context.OvertimePolicies
                .FirstOrDefaultAsync(p => p.DepartmentId == departmentId);

            if (policy == null)
                return true;

            double overtimeHours = totalWorkedHours - policy.StandardDailyHours;

            if (overtimeHours <= 0)
                return true;

            var approval = await _context.OvertimeApprovals
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.IsApproved &&
                    a.ValidFrom <= today &&
                    a.ValidTo >= today);

            if (approval == null)
                return true;

            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            double weeklyOvertime = await _context.OvertimeRecords
                .Where(o => o.UserId == userId &&
                            o.Date >= weekStart &&
                            o.Date <= today)
                .SumAsync(o => o.OvertimeHours);

            if (weeklyOvertime + overtimeHours > policy.MaxWeeklyOvertimeHours)
                return true;

            _context.OvertimeRecords.Add(new OvertimeRecord
            {
                UserId = userId,
                Date = today,
                OvertimeHours = overtimeHours
            });

            await _context.SaveChangesAsync();

            return true;
        }

        // =========================================
        // ✅ Get Today's All Sessions
        // =========================================
        public async Task<List<Attendance>> GetTodaySessionsAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Attendances
                .Where(a => a.UserId == userId &&
                            a.AttendanceDate == today)
                .OrderBy(a => a.CheckInTime)
                .ToListAsync();
        }

        // =========================================
        // ✅ Attendance History
        // =========================================
        public async Task<List<Attendance>> GetAttendanceHistoryAsync(int userId)
        {
            return await _context.Attendances
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CheckInTime)
                .ToListAsync();
        }
    }
}