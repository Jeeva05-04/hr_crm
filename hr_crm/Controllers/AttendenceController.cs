using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;

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

        [HttpPost("check-in")]
        public IActionResult CheckIn([FromQuery] int employeeId)
        {
            var connStr = _config.GetConnectionString("HR_CRM");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var checkSql = @"
        SELECT COUNT(*)
        FROM attendance
        WHERE employee_id = @empId
        AND attendance_date = CURRENT_DATE;
    ";

            using var checkCmd = new NpgsqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("empId", employeeId);

            int count = Convert.ToInt32(checkCmd.ExecuteScalar());
            if (count > 0)
                return BadRequest("Already checked in today");
            var insertSql = @"
INSERT INTO attendance
(employee_id, attendance_date, check_in_time, status)
VALUES
(@empId, CURRENT_DATE, CURRENT_TIME, 'Present');
";


            using var insertCmd = new NpgsqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("empId", employeeId);

            try
            {
                insertCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


            return Ok("Check-in successful");
        }
        [HttpPost("check-out")]
        public IActionResult CheckOut([FromQuery] int employeeId)
        {
            string connStr = _config.GetConnectionString("HR_CRM");

            using (var conn = new NpgsqlConnection(connStr))
            {
                conn.Open();

                string sql =
                    "UPDATE attendance " +
                    "SET check_out_time = CURRENT_TIME, " +
                    "total_hours = CURRENT_TIMESTAMP - (attendance_date + check_in_time) " +
                    "WHERE employee_id = @empId " +
                    "AND attendance_date = CURRENT_DATE " +
                    "AND check_out_time IS NULL;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("empId", employeeId);

                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                        return BadRequest("No active check-in found");

                    return Ok("Check-out successful");
                }
            }
        }

        [HttpGet("total-hours")]
        public IActionResult GetTotalHours([FromQuery] int employeeId)
        {
            var connStr = _config.GetConnectionString("HR_CRM");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
        SELECT
            employee_id,
            attendance_date,
            check_in_time,
            check_out_time,
            total_hours
        FROM attendance
        WHERE employee_id = @empId
        AND attendance_date = CURRENT_DATE;
    ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("empId", employeeId);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return NotFound("Attendance record not found for today");

            return Ok(new
            {
                EmployeeId = reader.GetInt32(0),
                Date = reader.GetDateTime(1).ToString("yyyy-MM-dd"),
                CheckIn = reader.IsDBNull(2) ? null : reader.GetTimeSpan(2).ToString(),
                CheckOut = reader.IsDBNull(3) ? null : reader.GetTimeSpan(3).ToString(),
                TotalHours = reader.IsDBNull(4) ? null : reader.GetTimeSpan(4).ToString()
            });
        }





    }
}


