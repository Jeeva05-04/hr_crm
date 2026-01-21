using Microsoft.AspNetCore.Mvc;
using Npgsql;
using hr_crm.Models;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly IConfiguration _config;

        public BranchController(IConfiguration config)
        {
            _config = config;
        }

        // =====================================
        // GET: api/branch
        // =====================================
        [HttpGet]
        public IActionResult GetBranches()
        {
            var connStr = _config.GetConnectionString("HR_CRM");
            var branches = new List<object>();

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                SELECT branch_id, branch_name, location, status
                FROM branches
                ORDER BY branch_name;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                branches.Add(new
                {
                    BranchId = reader.GetInt32(0),
                    BranchName = reader.GetString(1),
                    Location = reader.GetString(2),
                    Status = reader.GetString(3)
                });
            }

            return Ok(branches);
        }

        // =====================================
        // POST: api/branch
        // =====================================
        [HttpPost]
        public IActionResult AddBranch([FromBody] BranchCreateDto branch)
        {
            var connStr = _config.GetConnectionString("HR_CRM");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                INSERT INTO branches (branch_name, location, status)
                VALUES (@name, @loc, @status);
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", branch.BranchName);
            cmd.Parameters.AddWithValue("loc", branch.Location);
            cmd.Parameters.AddWithValue("status", branch.Status);

            cmd.ExecuteNonQuery();

            return Ok("Branch added successfully");
        }

        // =====================================
        // PUT: api/branch/{id}
        // =====================================
        [HttpPut("{id}")]
        public IActionResult UpdateBranch(int id, [FromBody] BranchCreateDto branch)
        {
            var connStr = _config.GetConnectionString("HR_CRM");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                UPDATE branches
                SET
                    branch_name = @name,
                    location = @loc,
                    status = @status
                WHERE branch_id = @id;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", branch.BranchName);
            cmd.Parameters.AddWithValue("loc", branch.Location);
            cmd.Parameters.AddWithValue("status", branch.Status);
            cmd.Parameters.AddWithValue("id", id);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
                return NotFound("Branch not found");

            return Ok("Branch updated successfully");
        }
    }
}
