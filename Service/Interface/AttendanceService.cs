using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;
using hr_crm.DTO;

namespace hr_crm.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;
        private readonly IIpGeolocationService _geoService;

        public AttendanceService(IAttendanceRepository repo, IIpGeolocationService geoService)
        {
            _repo = repo;
            _geoService = geoService;
        }

        // =========================================
        // ✅ Check-In (Create new session)
        // =========================================
        public async Task<(Attendance? Record, string? Error)> CheckInAsync(AttendanceCheckInDto dto, HttpContext httpContext)
        {
            // Get IP Address (handle proxy headers)
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim();
            }

            // Get Device Info
            var deviceInfo = httpContext.Request.Headers["User-Agent"].ToString();
            if (string.IsNullOrEmpty(deviceInfo))
                deviceInfo = "Unknown Device";

            // Use GPS coords sent by frontend; fall back to IP geolocation if not provided
            double? latitude = dto.Latitude;
            double? longitude = dto.Longitude;

            if (latitude == null || longitude == null)
            {
                var (ipLat, ipLon, _, _) = await _geoService.GetLocationAsync(ipAddress ?? "");
                latitude = ipLat;
                longitude = ipLon;
            }

            var (success, error) = await _repo.CheckInAsync(
                dto.UserId,
                ipAddress,
                latitude,
                longitude,
                deviceInfo
            );

            if (!success)
                return (null, error);

            return (new Attendance
            {
                UserId = dto.UserId,
                CheckInTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                DeviceInfo = deviceInfo,
                CheckInLatitude = latitude,
                CheckInLongitude = longitude,
                LastKnownLatitude = latitude,
                LastKnownLongitude = longitude,
                LastLocationUpdated = latitude.HasValue ? DateTime.UtcNow : null
            }, null);
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

        // =========================================
        // ✅ Update Live Location
        // =========================================
        public async Task<bool> UpdateLocationAsync(int userId, double latitude, double longitude)
        {
            return await _repo.UpdateLocationAsync(userId, latitude, longitude);
        }

        // =========================================
        // ✅ Get All Active Check-Ins
        // =========================================
        public async Task<List<Attendance>> GetActiveCheckInsAsync()
        {
            return await _repo.GetActiveCheckInsAsync();
        }

        // =========================================
        // ✅ Get One Active Check-In
        // =========================================
        public async Task<Attendance?> GetActiveCheckInAsync(int userId)
        {
            return await _repo.GetActiveCheckInAsync(userId);
        }

        // =========================================
        // ✅ Get Location Trail for a Day
        // =========================================
        public async Task<List<EmployeeLocationTrail>> GetLocationTrailAsync(int userId, DateTime date)
        {
            return await _repo.GetLocationTrailAsync(userId, date);
        }
    }
}