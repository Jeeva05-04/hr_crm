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
        // Check-In (STRICT SHIFT VALIDATION)
        // =========================================
        public async Task<(bool Success, string? Error)> CheckInAsync(int userId, string? ipAddress, double? latitude, double? longitude, string? deviceInfo)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            var userShift = await _context.UserShifts
                .Include(us => us.Shift)
                .FirstOrDefaultAsync(us => us.UserId == userId);

            if (userShift == null)
                return (false, "No shift assigned to this user. Please contact HR.");

            // Check if open session exists
            var openSession = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.AttendanceDate == today &&
                    a.CheckOutTime == null);

            if (openSession != null)
                return (false, "You are already checked in. Please check out first.");

            var newSession = new Attendance
            {
                UserId = userId,
                AttendanceDate = today,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = null,
                Status = "Present",

                // Store IP and Device Info
                IpAddress = ipAddress,
                DeviceInfo = deviceInfo,

                // Store check-in location
                CheckInLatitude = latitude,
                CheckInLongitude = longitude,
                LastKnownLatitude = latitude,
                LastKnownLongitude = longitude,
                LastLocationUpdated = latitude.HasValue ? DateTime.UtcNow : null
            };

            _context.Attendances.Add(newSession);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        // =========================================
        // Check-Out (Close Active Session + Overtime Engine)
        // =========================================
        public async Task<bool> CheckOutAsync(int userId)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

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
                .Sum(s => (s.CheckOutTime!.Value - s.CheckInTime).TotalHours);

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
        // Get Today's All Sessions
        // =========================================
        public async Task<List<Attendance>> GetTodaySessionsAsync(int userId)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            return await _context.Attendances
                .Where(a => a.UserId == userId &&
                            a.AttendanceDate == today)
                .OrderBy(a => a.CheckInTime)
                .ToListAsync();
        }

        // =========================================
        // Attendance History
        // =========================================
        public async Task<List<Attendance>> GetAttendanceHistoryAsync(int userId)
        {
            return await _context.Attendances
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CheckInTime)
                .ToListAsync();
        }

        // =========================================
        // Update Live Location + Save Trail Point
        // =========================================
        public async Task<bool> UpdateLocationAsync(int userId, double latitude, double longitude)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            var openSession = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.AttendanceDate == today &&
                    a.CheckOutTime == null);

            if (openSession == null)
                return false;

            var now = DateTime.UtcNow;

            // Update the current "last known" position on the attendance record
            openSession.LastKnownLatitude = latitude;
            openSession.LastKnownLongitude = longitude;
            openSession.LastLocationUpdated = now;

            // Append a trail point so we can replay the full movement path
            _context.EmployeeLocationTrails.Add(new EmployeeLocationTrail
            {
                AttendanceId = openSession.AttendanceId,
                UserId = userId,
                Latitude = latitude,
                Longitude = longitude,
                RecordedAt = now
            });

            await _context.SaveChangesAsync();
            return true;
        }

        // =========================================
        // Get Full Location Trail for a Day
        // =========================================
        public async Task<List<EmployeeLocationTrail>> GetLocationTrailAsync(int userId, DateTime date)
        {
            var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var utcNextDay = utcDate.AddDays(1);

            return await _context.EmployeeLocationTrails
                .Where(t => t.UserId == userId && t.RecordedAt >= utcDate && t.RecordedAt < utcNextDay)
                .OrderBy(t => t.RecordedAt)
                .ToListAsync();
        }

        // =========================================
        // Get All Active Check-Ins (for managers)
        // =========================================
        public async Task<List<Attendance>> GetActiveCheckInsAsync()
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            return await _context.Attendances
                .Where(a => a.AttendanceDate == today && a.CheckOutTime == null)
                .OrderBy(a => a.UserId)
                .ToListAsync();
        }

        // =========================================
        // Get One Active Check-In
        // =========================================
        public async Task<Attendance?> GetActiveCheckInAsync(int userId)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            return await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.AttendanceDate == today &&
                    a.CheckOutTime == null);
        }
    }
}