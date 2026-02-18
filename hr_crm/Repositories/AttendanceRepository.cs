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
        // Mark Daily Attendance For All Employees
        // =========================================
        public async Task MarkDailyAttendanceAsync()
        {
            var today = DateTime.UtcNow.Date;
            var employees = await _context.Employees.ToListAsync();

            foreach (var emp in employees)
            {
                bool exists = await _context.Attendances
                    .AnyAsync(a => a.EmployeeId == emp.EmployeeId
                                && a.AttendanceDate == today);

                if (!exists)
                {
                    _context.Attendances.Add(new Attendance
                    {
                        EmployeeId = emp.EmployeeId,
                        AttendanceDate = today,
                        Status = "Present"
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // =========================================
        // Update Today's Attendance Status
        // =========================================
        public async Task<bool> UpdateAttendanceAsync(int employeeId, string status)
        {
            var today = DateTime.UtcNow.Date;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                       && a.AttendanceDate == today);

            if (attendance == null)
                return false;

            attendance.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        // =========================================
        // Get Today's Attendance (All Employees)
        // =========================================
        public async Task<List<Attendance>> GetTodayAttendanceAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.AttendanceDate == today)
                .ToListAsync();
        }

        // =========================================
        // Check-In
        // =========================================
        public async Task<bool> CheckInAsync(int employeeId)
        {
            var today = DateTime.UtcNow.Date;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                       && a.AttendanceDate == today);

            if (attendance != null && attendance.CheckInTime != null)
                return false; // Already checked in

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    EmployeeId = employeeId,
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
        public async Task<bool> CheckOutAsync(int employeeId)
        {
            var today = DateTime.UtcNow.Date;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
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
        // Get Today's Record For Employee
        // =========================================
        public async Task<Attendance?> GetTodayRecordAsync(int employeeId)
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                       && a.AttendanceDate == today);
        }

        // =========================================
        // Get Full Attendance History (Past Records)
        // =========================================
        public async Task<List<Attendance>> GetAttendanceHistoryAsync(int employeeId)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }
    }
}
