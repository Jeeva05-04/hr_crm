using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
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
        // Mark Daily Attendance (For All Employees)
        // =========================================
        [HttpPost("daily")]
        public async Task<IActionResult> MarkDailyAttendance()
        {
            await _service.MarkDailyAttendanceAsync();
            return Ok("Daily attendance marked");
        }

        // =========================================
        // Update Attendance Status
        // =========================================
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAttendance(int employeeId, string status)
        {
            var result = await _service.UpdateAttendanceAsync(employeeId, status);

            if (!result)
                return BadRequest("Attendance record not found");

            return Ok("Attendance updated successfully");
        }

        // =========================================
        // Get Today Attendance (All Employees)
        // =========================================
        [HttpGet("today")]
        public async Task<IActionResult> GetTodayAttendance()
        {
            var data = await _service.GetTodayAttendanceAsync();

            var result = data.Select(a => new
            {
                a.EmployeeId,
                a.Employee.FirstName,
                a.AttendanceDate,
                a.Status,
                a.CheckInTime,
                a.CheckOutTime,
                a.TotalHours
            });

            return Ok(result);
        }

        // =========================================
        // Check-In
        // =========================================
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn(int employeeId)
        {
            var success = await _service.CheckInAsync(employeeId);

            if (!success)
                return BadRequest("Already checked in today");

            return Ok("Check-in successful");
        }

        // =========================================
        // Check-Out
        // =========================================
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut(int employeeId)
        {
            var success = await _service.CheckOutAsync(employeeId);

            if (!success)
                return BadRequest("No active check-in found");

            return Ok("Check-out successful");
        }

        // =========================================
        // Get Today Total Hours (Single Employee)
        // =========================================
        [HttpGet("total-hours")]
        public async Task<IActionResult> GetTotalHours(int employeeId)
        {
            var record = await _service.GetTodayRecordAsync(employeeId);

            if (record == null)
                return NotFound("Attendance record not found for today");

            return Ok(new
            {
                record.EmployeeId,
                record.AttendanceDate,
                record.CheckInTime,
                record.CheckOutTime,
                record.TotalHours
            });
        }

        // =========================================
        // Get Attendance History (Past Records)
        // =========================================
        [HttpGet("history/{employeeId}")]
        public async Task<IActionResult> GetAttendanceHistory(int employeeId)
        {
            var records = await _service.GetAttendanceHistoryAsync(employeeId);

            if (records == null || !records.Any())
                return NotFound("No attendance history found");

            var result = records.Select(r => new
            {
                r.EmployeeId,
                r.AttendanceDate,
                r.Status,
                r.CheckInTime,
                r.CheckOutTime,
                r.TotalHours
            });

            return Ok(result);
        }
    }
}
