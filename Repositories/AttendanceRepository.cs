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
        // Mark Daily Attendance (Does NOT depend on Employees anymore)
        // =========================================
        public async Task MarkDailyAttendanceAsync()
        {
            var today = DateTime.UtcNow.Date;

            // If you want bulk marking, you must pass user list externally.
            // Since Users are in CRM database, we cannot auto-fetch them here.

            await Task.CompletedTask;
        }

        // =========================================
        // Update Attendance
        // =========================================
        public async Task<bool> UpdateAttendanceAsync(int userId, string status)
        {
            var today = DateTime.UtcNow.Date;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId
                                       && a.AttendanceDate == today);

            if (attendance == null)
                return false;

            attendance.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        // =========================================
        // Get Today Attendance
        // =========================================
        public async Task<List<Attendance>> GetTodayAttendanceAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Attendances
                .Where(a => a.AttendanceDate == today)
                .ToListAsync();
        }

        // =========================================
        // Check-In
        // =========================================
        public async Task<bool> CheckInAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId
                                       && a.AttendanceDate == today);

            if (attendance != null && attendance.CheckInTime != null)
                return false;

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    UserId = userId,
                    AttendanceDate = today,
                    Status = "Present"
                };

                _context.Attendances.Add(attendance);
            }

            attendance.CheckInTime = DateTime.Now.TimeOfDay;

            await _context.SaveChangesAsync();
            return true;
        }

        // =========================================
        // Check-Out
        // =========================================
        public async Task<bool> CheckOutAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId
                                       && a.AttendanceDate == today);

            if (attendance == null || attendance.CheckInTime == null || attendance.CheckOutTime != null)
                return false;

            attendance.CheckOutTime = DateTime.Now.TimeOfDay;

            attendance.TotalHours =
                attendance.CheckOutTime - attendance.CheckInTime;

            await _context.SaveChangesAsync();
            return true;
        }

        // =========================================
        // Get Today Record
        // =========================================
        public async Task<Attendance?> GetTodayRecordAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId
                                       && a.AttendanceDate == today);
        }

        // =========================================
        // Attendance History
        // =========================================
        public async Task<List<Attendance>> GetAttendanceHistoryAsync(int userId)
        {
            return await _context.Attendances
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }
    }
}
