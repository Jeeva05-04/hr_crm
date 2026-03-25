using hr_crm.Service.Interface;
using hr_crm.Service;
using hr_crm.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using hr_crm.DTO;
using System.Security.Claims;
using hr_crm.Extensions;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ensure only logged-in users can access
    public class AttendenceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IHubContext<LocationHub> _hubContext;
        private readonly LoggingService _loggingService;

        public AttendenceController(IAttendanceService attendanceService, IHubContext<LocationHub> hubContext, LoggingService loggingService)
        {
            _attendanceService = attendanceService;
            _hubContext = hubContext;
            _loggingService = loggingService;
        }

        // Helper to call logging service without throwing
        private async Task _logging_service_safe(int? userId, string? userName, string action, string details)
        {
            try
            {
                if (_loggingService != null)
                    await _loggingService.CreateLog(userId, userName, action, details);
            }
            catch { }
        }

        // =========================================
        // Check-In
        // =========================================
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] AttendanceCheckInDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            var tokenUserId = int.Parse(userIdClaim.Value);

            // Prevent checking in for another user
            if (dto.UserId != tokenUserId)
                return Forbid("You cannot check-in for another user.");

            // HttpContext will be used to capture IP & Device info
            var (record, error) = await _attendanceService.CheckInAsync(dto, HttpContext);

            if (record == null)
                return BadRequest(new { Message = error });

            // Create a semantic log for the check-in
            try
            {
                var userName = User.GetDisplayName();
                var details = $"Ip={record.IpAddress}; Device={record.DeviceInfo}; Lat={record.CheckInLatitude}; Lon={record.CheckInLongitude}";
                await _logging_service_safe(tokenUserId, userName, "CheckIn", details);
            }
            catch { }

            return Ok(new
            {
                Message = "Check-in successful",
                record.UserId,
                record.CheckInTime,
                record.IpAddress,
                record.DeviceInfo,
                record.CheckInLatitude,
                record.CheckInLongitude
            });
        }


        // =========================================
        // Check-Out
        // =========================================
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut(int userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            var tokenUserId = int.Parse(userIdClaim.Value);

            if (userId != tokenUserId)
                return Forbid("You cannot check-out another user.");

            var result = await _attendanceService.CheckOutAsync(userId);

            if (!result)
                return BadRequest("No active check-in found");

            try
            {
                var userName = User.GetDisplayName();
                await _logging_service_safe(userId, userName, "CheckOut", $"User checked out at {DateTime.UtcNow}");
            }
            catch { }

            return Ok(new
            {
                Message = "Check-out successful",
                UserId = userId,
                Time = DateTime.UtcNow
            });
        }


        // =========================================
        // Total Hours Today
        // =========================================
        [HttpGet("total-hours")]
        public async Task<IActionResult> GetTotalHours(int userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            var tokenUserId = int.Parse(userIdClaim.Value);

            // Check if user is HR
            var isHR = User.IsInRole("HR_USER") || User.IsInRole("HR_MANAGER");

            // Employees can only view their own hours
            if (!isHR && userId != tokenUserId)
                return Forbid("You can only view your own data.");

            // Get attendance history
            var records = await _attendanceService.GetAttendanceHistoryAsync(userId);

            if (records == null || !records.Any())
                return NotFound("No attendance history found");

            var result = records
                .GroupBy(r => r.AttendanceDate)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalHours = g
                        .Where(x => x.CheckOutTime != null)
                        .Sum(x => (x.CheckOutTime.Value - x.CheckInTime).TotalHours)
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            return Ok(new
            {
                UserId = userId,
                TotalHoursHistory = result
            });
        }


        // =========================================
        // Attendance History
        // =========================================
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            var tokenUserId = int.Parse(userIdClaim.Value);

            // Check if user is HR
            var isHR = User.IsInRole("HR_USER") || User.IsInRole("HR_MANAGER");

            // Employees can only see their own data
            if (!isHR && userId != tokenUserId)
                return Forbid("You can only view your own attendance history.");

            var records = await _attendanceService.GetAttendanceHistoryAsync(userId);

            if (records == null || !records.Any())
                return NotFound("No attendance history found");

            return Ok(records);
        }


        // =========================================
        // Update Live Location (called by employee app periodically)
        // =========================================
        [HttpPut("location")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            var tokenUserId = int.Parse(userIdClaim.Value);

            if (dto.UserId != tokenUserId)
                return Forbid("You cannot update location for another user.");

            var updated = await _attendanceService.UpdateLocationAsync(dto.UserId, dto.Latitude, dto.Longitude);

            if (!updated)
                return BadRequest("No active check-in found. Please check in first.");

            // Broadcast real-time update to all connected HR managers
            await _hubContext.Clients.Group("managers").SendAsync("EmployeeLocationUpdated", new
            {
                UserId = dto.UserId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                LastUpdated = DateTime.UtcNow,
                GoogleMapsLink = $"https://www.google.com/maps?q={dto.Latitude},{dto.Longitude}"
            });
            // Log the location update (best-effort)
            try
            {
                var userName = User.GetDisplayName();
                var details = $"Lat={dto.Latitude}; Lon={dto.Longitude}";
                await _logging_service_safe(dto.UserId, userName, "LocationUpdate", details);
            }
            catch { }

            return Ok(new
            {
                Message = "Location updated",
                UserId = dto.UserId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                UpdatedAt = DateTime.UtcNow,
                GoogleMapsLink = $"https://www.google.com/maps?q={dto.Latitude},{dto.Longitude}"
            });
        }


        // =========================================
        // Live Locations - All Checked-In Employees (Manager/HR only)
        // =========================================
        [HttpGet("live-locations")]
        public async Task<IActionResult> GetLiveLocations()
        {
            var isHR = User.IsInRole("HR_USER") || User.IsInRole("HR_MANAGER");

            if (!isHR)
                return Forbid("Only HR managers can view live locations.");

            var activeCheckIns = await _attendanceService.GetActiveCheckInsAsync();

            if (!activeCheckIns.Any())
                return Ok(new { Message = "No employees currently checked in.", Employees = new List<object>() });

            var result = activeCheckIns.Select(a => new
            {
                a.UserId,
                a.CheckInTime,
                CheckInLocation = a.CheckInLatitude.HasValue
                    ? new
                    {
                        Latitude = a.CheckInLatitude,
                        Longitude = a.CheckInLongitude,
                        GoogleMapsLink = $"https://www.google.com/maps?q={a.CheckInLatitude},{a.CheckInLongitude}"
                    }
                    : null,
                LiveLocation = a.LastKnownLatitude.HasValue
                    ? new
                    {
                        Latitude = a.LastKnownLatitude,
                        Longitude = a.LastKnownLongitude,
                        LastUpdated = a.LastLocationUpdated,
                        GoogleMapsLink = $"https://www.google.com/maps?q={a.LastKnownLatitude},{a.LastKnownLongitude}"
                    }
                    : null,
                a.IpAddress,
                a.DeviceInfo,
                a.Status
            });

            return Ok(result);
        }


        // =========================================
        // Location Trail - Full movement path for today or a specific date
        // GET /api/attendance/location-trail/{userId}           → today (HR only)
        // GET /api/attendance/location-trail/{userId}?date=2026-03-14  → specific date (HR only)
        // =========================================
        [HttpGet("location-trail/{userId}")]
        public async Task<IActionResult> GetLocationTrail(int userId, [FromQuery] DateTime? date)
        {
            var isHR = User.IsInRole("HR_USER") || User.IsInRole("HR_MANAGER");
            if (!isHR)
                return Forbid("Only HR managers can view location trails.");

            var targetDate = date?.Date ?? DateTime.UtcNow.Date;

            var trail = await _attendanceService.GetLocationTrailAsync(userId, targetDate);

            if (!trail.Any())
                return Ok(new { UserId = userId, Date = targetDate, Trail = new List<object>(), Message = "No location data recorded for this day." });

            var result = trail.Select((point, index) => new
            {
                Step = index + 1,
                point.Latitude,
                point.Longitude,
                point.RecordedAt,
                GoogleMapsLink = $"https://www.google.com/maps?q={point.Latitude},{point.Longitude}"
            }).ToList();

            return Ok(new
            {
                UserId = userId,
                Date = targetDate,
                TotalPoints = trail.Count,
                StartLocation = new { result.First().Latitude, result.First().Longitude, result.First().RecordedAt },
                CurrentLocation = new { result.Last().Latitude, result.Last().Longitude, result.Last().RecordedAt },
                Trail = result
            });
        }


        // =========================================
        // Live Location - Single Employee (Manager/HR or self)
        // =========================================
        [HttpGet("live-location/{userId}")]
        public async Task<IActionResult> GetUserLiveLocation(int userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            var tokenUserId = int.Parse(userIdClaim.Value);
            var isHR = User.IsInRole("HR_USER") || User.IsInRole("HR_MANAGER");

            if (!isHR)
                return Forbid("Only HR managers can view live locations.");

            var session = await _attendanceService.GetActiveCheckInAsync(userId);

            if (session == null)
                return NotFound("User is not currently checked in.");

            return Ok(new
            {
                session.UserId,
                session.CheckInTime,
                CheckInLocation = session.CheckInLatitude.HasValue
                    ? new
                    {
                        Latitude = session.CheckInLatitude,
                        Longitude = session.CheckInLongitude,
                        GoogleMapsLink = $"https://www.google.com/maps?q={session.CheckInLatitude},{session.CheckInLongitude}"
                    }
                    : null,
                LiveLocation = session.LastKnownLatitude.HasValue
                    ? new
                    {
                        Latitude = session.LastKnownLatitude,
                        Longitude = session.LastKnownLongitude,
                        LastUpdated = session.LastLocationUpdated,
                        GoogleMapsLink = $"https://www.google.com/maps?q={session.LastKnownLatitude},{session.LastKnownLongitude}"
                    }
                    : null,
                session.Status
            });
        }
    }
}