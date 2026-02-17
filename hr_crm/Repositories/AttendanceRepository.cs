using hr_crm.Data;
using hr_crm.Entities;
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

        public async Task MarkDailyAttendanceAsync()
        {
            var today = DateTime.Today;
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

        public async Task<bool> UpdateAttendanceAsync(int employeeId, string status)
        {
            var today = DateTime.Today;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                       && a.AttendanceDate == today);

            if (attendance == null)
                return false;

            attendance.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Attendance>> GetTodayAttendanceAsync()
        {
            var today = DateTime.Today;

            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.AttendanceDate == today)
                .ToListAsync();
        }

        public async Task<bool> CheckInAsync(int employeeId)
        {
            var today = DateTime.Today;

            bool exists = await _context.Attendances
                .AnyAsync(a => a.EmployeeId == employeeId
                            && a.AttendanceDate == today);

            if (exists)
                return false;

            _context.Attendances.Add(new Attendance
            {
                EmployeeId = employeeId,
                AttendanceDate = today,
                CheckInTime = DateTime.Now.TimeOfDay,
                Status = "Present"
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckOutAsync(int employeeId)
        {
            var today = DateTime.Today;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                       && a.AttendanceDate == today);

            if (attendance == null || attendance.CheckOutTime != null)
                return false;

            attendance.CheckOutTime = DateTime.Now.TimeOfDay;

            if (attendance.CheckInTime.HasValue)
                attendance.TotalHours =
                    attendance.CheckOutTime - attendance.CheckInTime;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Attendance?> GetTodayRecordAsync(int employeeId)
        {
            var today = DateTime.Today;

            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                       && a.AttendanceDate == today);
        }
    }
}
