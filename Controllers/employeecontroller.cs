using hr_crm.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
        {
            _config = config;
        }


        [HttpGet]
        public IActionResult GetEmployees()
        {
            var connStr = _config.GetConnectionString("HR_CRM");

            if (string.IsNullOrEmpty(connStr))
                return StatusCode(500, "Connection string not found");

            var employees = new List<object>();

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
        SELECT 
            employee_id,
            first_name,
            email,
            phone,
            emergency_contact,
            department_id,
            designation,
            address,
            date_of_joining,
            salary,
            status
        FROM employees
        ORDER BY employee_id;
    ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                employees.Add(new
                {
                    EmployeeId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    Email = reader.GetString(2),
                    Phone = reader.GetString(3),
                    EmergencyContact = reader.GetString(4),
                    DepartmentId = reader.GetInt32(5),
                    Designation = reader.GetString(6),
                    Address = reader.GetString(7),
                    DateOfJoining = reader.GetDateTime(8),
                    Salary = reader.GetDecimal(9),
                    Status = reader.GetString(10)
                });
            }

            return Ok(employees);
        }




        [HttpPost]
        public IActionResult AddEmployee([FromBody] EmployeeCreateDto emp)
        {
            var connStr = _config.GetConnectionString("HR_CRM");

            if (string.IsNullOrEmpty(connStr))
                return StatusCode(500, "Connection string not found");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
        INSERT INTO employees
        (first_name, last_name, email, phone, emergency_contact,
         department_id, designation, address, date_of_joining, salary, status)
        VALUES
        (@fn, @ln, @em, @ph, @ec, @did, @des, @addr, CURRENT_DATE, @sal, 'Active');
    ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("fn", emp.FirstName);
            cmd.Parameters.AddWithValue("ln", emp.LastName);
            cmd.Parameters.AddWithValue("em", emp.Email);
            cmd.Parameters.AddWithValue("ph", emp.Phone);
            cmd.Parameters.AddWithValue("ec", emp.EmergencyContact);
            cmd.Parameters.AddWithValue("did", emp.DepartmentId);
            cmd.Parameters.AddWithValue("des", emp.Designation);
            cmd.Parameters.AddWithValue("addr", emp.Address);
            cmd.Parameters.AddWithValue("sal", emp.Salary);

            cmd.ExecuteNonQuery();

            return Ok("Employee added successfully");
        }



    }
}

