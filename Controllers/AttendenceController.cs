using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace hr_crm.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendanceController(IAttendanceService service)
        {
            _service = service;
        }

        // =========================================
        // ✅ Check-In (Creates New Session)
        // =========================================
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn(int userId)
        {
            var result = await _service.CheckInAsync(userId);

            if (!result)
                return BadRequest("User already has active session");

            return Ok(new
            {
                Message = "Check-in successful",
                UserId = userId,
                Time = DateTime.UtcNow
            });
        }

        // =========================================
        // ✅ Check-Out (Closes Active Session)
        // =========================================
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut(int userId)
        {
            var result = await _service.CheckOutAsync(userId);

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
        // ✅ Get Today's Total Hours (Sum of Sessions)
        // =========================================
        [HttpGet("total-hours")]
        public async Task<IActionResult> GetTotalHours(int userId)
        {
            var totalHours = await _service.CalculateTodayTotalHoursAsync(userId);

            return Ok(new
            {
                UserId = userId,
                Date = DateTime.UtcNow.Date,
                TotalHours = totalHours
            });
        }

        // =========================================
        // ✅ Get Full History
        // =========================================
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            var records = await _service.GetAttendanceHistoryAsync(userId);

            if (records == null || !records.Any())
                return NotFound("No attendance history found");

            return Ok(records);
        }
    }
}