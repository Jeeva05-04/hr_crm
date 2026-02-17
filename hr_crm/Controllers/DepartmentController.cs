using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using hr_crm.Models;   // VERY IMPORTANT

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
        {
            _config = config;
        }

        // ==============================
        // GET: api/department
        // ==============================
        [HttpGet]
        public IActionResult GetDepartments()
        {
            var connStr = _config.GetConnectionString("HrDb");
            var departments = new List<object>();

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                SELECT d.department_id,
                       d.department_name,
                       d.branch_id,
                       b.branch_name
                FROM departments d
                JOIN branches b ON d.branch_id = b.branch_id
                ORDER BY d.department_name;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                departments.Add(new
                {
                    DepartmentId = reader.GetInt32(0),
                    DepartmentName = reader.GetString(1),
                    BranchId = reader.GetInt32(2),
                    BranchName = reader.GetString(3)
                });
            }

            return Ok(departments);
        }

        // ==============================
        // POST: api/department
        // ==============================
        [HttpPost]
        public IActionResult AddDepartment([FromBody] DepartmentDto dto)
        {
            var connStr = _config.GetConnectionString("HrDb");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                INSERT INTO departments (department_name, branch_id)
                VALUES (@name, @branchId);
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", dto.DepartmentName);
            cmd.Parameters.AddWithValue("branchId", dto.BranchId);

            cmd.ExecuteNonQuery();

            return Ok("Department added successfully");
        }

        // ==============================
        // PUT: api/department/{id}
        // ==============================
        [HttpPut("{id}")]
        public IActionResult UpdateDepartment(int id, [FromBody] DepartmentDto dto)
        {
            var connStr = _config.GetConnectionString("HrDb");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                UPDATE departments
                SET department_name = @name,
                    branch_id = @branchId
                WHERE department_id = @id;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", dto.DepartmentName);
            cmd.Parameters.AddWithValue("branchId", dto.BranchId);
            cmd.Parameters.AddWithValue("id", id);

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return NotFound("Department not found");

            return Ok("Department updated successfully");
        }

        // ==============================
        // DELETE: api/department/{id}
        // ==============================
        [HttpDelete("{id}")]
        public IActionResult DeleteDepartment(int id)
        {
            var connStr = _config.GetConnectionString("HrDb");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = "DELETE FROM departments WHERE department_id = @id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return NotFound("Department not found");

            return Ok("Department deleted successfully");
        }
    }
}
