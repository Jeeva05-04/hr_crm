using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hr_crm.DTO;
using System.Security.Claims;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ensure only logged-in users can access
    public class AttendenceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendenceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
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
            var result = await _attendanceService.CheckInAsync(dto, HttpContext);

            return Ok(result);
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

            if (userId != tokenUserId)
                return Forbid("You can only view your own data.");

            var totalHours = await _attendanceService.CalculateTodayTotalHoursAsync(userId);

            return Ok(new
            {
                UserId = userId,
                Date = DateTime.UtcNow.Date,
                TotalHours = totalHours
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

            if (userId != tokenUserId)
                return Forbid("You can only view your own attendance history.");

            var records = await _attendanceService.GetAttendanceHistoryAsync(userId);

            if (records == null || !records.Any())
                return NotFound("No attendance history found");

            return Ok(records);
        }
    }
}