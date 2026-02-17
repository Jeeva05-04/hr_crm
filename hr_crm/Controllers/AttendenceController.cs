using hr_crm.Services;
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

        [HttpPost("daily")]
        public async Task<IActionResult> MarkDailyAttendance()
        {
            await _service.MarkDailyAttendanceAsync();
            return Ok("Daily attendance marked");
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAttendance(int employeeId, string status)
        {
            var result = await _service.UpdateAttendanceAsync(employeeId, status);
            if (!result)
                return BadRequest("Attendance not found");

            return Ok("Updated successfully");
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodayAttendance()
        {
            var data = await _service.GetTodayAttendanceAsync();

            var result = data.Select(a => new
            {
                a.EmployeeId,
                a.Employee.FirstName,
                a.AttendanceDate,
                a.Status
            });

            return Ok(result);
        }

        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn(int employeeId)
        {
            var success = await _service.CheckInAsync(employeeId);
            if (!success)
                return BadRequest("Already checked in");

            return Ok("Check-in successful");
        }

        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut(int employeeId)
        {
            var success = await _service.CheckOutAsync(employeeId);
            if (!success)
                return BadRequest("No active check-in found");

            return Ok("Check-out successful");
        }

        [HttpGet("total-hours")]
        public async Task<IActionResult> GetTotalHours(int employeeId)
        {
            var record = await _service.GetTodayRecordAsync(employeeId);

            if (record == null)
                return NotFound("Record not found");

            return Ok(new
            {
                record.EmployeeId,
                record.AttendanceDate,
                record.CheckInTime,
                record.CheckOutTime,
                record.TotalHours
            });
        }
    }
}
