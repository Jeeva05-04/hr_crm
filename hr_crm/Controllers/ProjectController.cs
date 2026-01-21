using Microsoft.AspNetCore.Mvc;
using Npgsql;
using hr_crm.Models;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProjectController(IConfiguration config)
        {
            _config = config;
        }

        // =====================================
        // GET: api/project
        // =====================================
        [HttpGet]
        public IActionResult GetProjects()
        {
            var connStr = _config.GetConnectionString("HR_CRM");
            var projects = new List<object>();

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                SELECT 
                    p.project_id,
                    p.project_name,
                    p.duration,
                    p.status,
                    e.first_name AS manager_name,
                    d.department_name,
                    p.created_date
                FROM projects p
                JOIN employees e ON p.manager_id = e.employee_id
                JOIN departments d ON p.department_id = d.department_id
                ORDER BY p.created_date DESC;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                projects.Add(new
                {
                    ProjectId = reader.GetInt32(0),
                    ProjectName = reader.GetString(1),
                    Duration = reader.GetString(2),
                    Status = reader.GetString(3),
                    ManagerName = reader.GetString(4),
                    DepartmentName = reader.GetString(5),
                    CreatedDate = reader.GetDateTime(6)
                });
            }

            return Ok(projects);
        }

        // =====================================
        // POST: api/project
        // =====================================
        [HttpPost]
        [Consumes("application/json")]
        public IActionResult AddProject([FromBody] ProjectCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var connStr = _config.GetConnectionString("HR_CRM");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                INSERT INTO projects
                (project_name, duration, status, manager_id, department_id)
                VALUES
                (@name, @duration, @status, @manager, @dept);
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", dto.ProjectName);
            cmd.Parameters.AddWithValue("duration", dto.Duration);
            cmd.Parameters.AddWithValue("status", dto.Status);
            cmd.Parameters.AddWithValue("manager", dto.ManagerId);
            cmd.Parameters.AddWithValue("dept", dto.DepartmentId);

            cmd.ExecuteNonQuery();

            return Ok("Project created successfully");
        }

        // =====================================
        // PUT: api/project/{id}
        // =====================================
        [HttpPut("{id}")]
        [Consumes("application/json")]
        public IActionResult UpdateProject(int id, [FromBody] ProjectCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var connStr = _config.GetConnectionString("HR_CRM");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                UPDATE projects
                SET
                    project_name = @name,
                    duration = @duration,
                    status = @status,
                    manager_id = @manager,
                    department_id = @dept
                WHERE project_id = @id;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", dto.ProjectName);
            cmd.Parameters.AddWithValue("duration", dto.Duration);
            cmd.Parameters.AddWithValue("status", dto.Status);
            cmd.Parameters.AddWithValue("manager", dto.ManagerId);
            cmd.Parameters.AddWithValue("dept", dto.DepartmentId);
            cmd.Parameters.AddWithValue("id", id);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
                return NotFound("Project not found");

            return Ok("Project updated successfully");
        }
    }
}
