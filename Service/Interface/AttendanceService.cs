using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;
using hr_crm.DTO;

namespace hr_crm.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;

        public AttendanceService(IAttendanceRepository repo)
        {
            _repo = repo;
        }

        // =========================================
        // ✅ Check-In (Create new session)
        // =========================================
        public async Task<Attendance> CheckInAsync(AttendanceCheckInDto dto, HttpContext httpContext)
        {
            // Get IP Address
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            // If behind proxy (Azure / Nginx)
            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            // Get Device Info
            var deviceInfo = httpContext.Request.Headers["User-Agent"].ToString();

            if (string.IsNullOrEmpty(deviceInfo))
            {
                deviceInfo = "Unknown Device";
            }

            await _repo.CheckInAsync(
                dto.UserId,
                ipAddress,
                null,   // latitude not used
                null,   // longitude not used
                deviceInfo
            );

            return new Attendance
            {
                UserId = dto.UserId,
                CheckInTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                DeviceInfo = deviceInfo
            };
        }

        // =========================================
        // ✅ Check-Out (Close open session)
        // =========================================
        public async Task<bool> CheckOutAsync(int userId)
        {
            return await _repo.CheckOutAsync(userId);
        }

        // =========================================
        // ✅ Calculate Today's Total Hours
        // =========================================
        public async Task<TimeSpan> CalculateTodayTotalHoursAsync(int userId)
        {
            var sessions = await _repo.GetTodaySessionsAsync(userId);

            TimeSpan total = TimeSpan.Zero;

            foreach (var session in sessions)
            {
                if (session.CheckOutTime != null)
                {
                    total += session.CheckOutTime.Value - session.CheckInTime;
                }
            }

            return total;
        }

        // =========================================
        // ✅ Get Full History
        // =========================================
        public async Task<List<Attendance>> GetAttendanceHistoryAsync(int userId)
        {
            return await _repo.GetAttendanceHistoryAsync(userId);
        }
    }
}