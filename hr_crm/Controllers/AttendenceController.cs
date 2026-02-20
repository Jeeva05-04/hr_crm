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
        // ✅ Check-In
        // =========================================
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn(int userId)
        {
            var success = await _service.CheckInAsync(userId);

            if (!success)
                return BadRequest("Already checked in or invalid user");

            return Ok(new
            {
                Message = "Check-in successful",
                UserId = userId
            });
        }

        // =========================================
        // ✅ Check-Out
        // =========================================
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut(int userId)
        {
            var success = await _service.CheckOutAsync(userId);

            if (!success)
                return BadRequest("No active check-in found");

            return Ok(new
            {
                Message = "Check-out successful",
                UserId = userId
            });
        }

        // =========================================
        // ✅ Get Total Hours
        // =========================================
        [HttpGet("total-hours")]
        public async Task<IActionResult> GetTotalHours(int userId)
        {
            var record = await _service.GetTodayRecordAsync(userId);

            if (record == null)
                return NotFound("No attendance record found");

            return Ok(new
            {
                record.UserId,
                record.AttendanceDate,
                record.CheckInTime,
                record.CheckOutTime,
                record.TotalHours
            });
        }

        // =========================================
        // ✅ Update Attendance Status
        // =========================================
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateStatus(int userId, string status)
        {
            var result = await _service.UpdateAttendanceAsync(userId, status);

            if (!result)
                return NotFound("Attendance record not found");

            return Ok(new
            {
                Message = "Attendance updated successfully",
                UserId = userId,
                Status = status
            });
        }

        // =========================================
        // ✅ Get History
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
