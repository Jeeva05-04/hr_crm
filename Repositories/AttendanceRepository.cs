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
        // ✅ Check-In (Create New Session)
        // =========================================
        public async Task<bool> CheckInAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;

            // Check if already checked-in and not checked-out
            var openSession = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.AttendanceDate == today &&
                    a.CheckOutTime == null);

            if (openSession != null)
                return false; // Already active session

            var newSession = new Attendance
            {
                UserId = userId,
                AttendanceDate = today,
                CheckInTime = DateTime.UtcNow,
                CheckOutTime = null,
                Status = "Present"
            };

            _context.Attendances.Add(newSession);
            await _context.SaveChangesAsync();

            return true;
        }

        // =========================================
        // ✅ Check-Out (Close Active Session)
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
        // ✅ Attendance History (All Sessions)
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