using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AttendanceController(IConfiguration config)
        {
            _config = config;
        }

        // =====================================================
        // 1️⃣ AUTO-MARK DAILY ATTENDANCE (ALL EMPLOYEES)
        // =====================================================
        // POST: api/attendance/daily
        [HttpPost("daily")]
        public IActionResult MarkDailyAttendance()
        {
            var connStr = _config.GetConnectionString("HR_CRM");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                INSERT INTO attendance (employee_id, attendance_date, status)
                SELECT employee_id, CURRENT_DATE, 'Present'
                FROM employees
                ON CONFLICT (employee_id, attendance_date)
                DO NOTHING;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.ExecuteNonQuery();

            return Ok("Daily attendance marked for all employees");
        }

        // =====================================================
        // 2️⃣ UPDATE ATTENDANCE (ABSENT / LEAVE)
        // =====================================================
        // PUT: api/attendance/update
        [HttpPut("update")]
        public IActionResult UpdateAttendance(int employeeId, string status)
        {
            var connStr = _config.GetConnectionString("HR_CRM");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                UPDATE attendance
                SET status = @status
                WHERE employee_id = @eid
                AND attendance_date = CURRENT_DATE;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("eid", employeeId);
            cmd.Parameters.AddWithValue("status", status);

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return BadRequest("Attendance not found for today");

            return Ok("Attendance updated successfully");
        }

        // =====================================================
        // 3️⃣ GET TODAY’S ATTENDANCE REPORT
        // =====================================================
        // GET: api/attendance/today
        [HttpGet("today")]
        public IActionResult GetTodayAttendance()
        {
            var connStr = _config.GetConnectionString("HR_CRM");
            var list = new List<object>();

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                SELECT 
                    e.employee_id,
                    e.first_name,
                    a.attendance_date,
                    a.status
                FROM attendance a
                JOIN employees e
                    ON a.employee_id = e.employee_id
                WHERE a.attendance_date = CURRENT_DATE
                ORDER BY e.employee_id;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new
                {
                    EmployeeId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    AttendanceDate = reader.GetDateTime(2),
                    Status = reader.GetString(3)
                });
            }

            return Ok(list);
        }
    }
}
