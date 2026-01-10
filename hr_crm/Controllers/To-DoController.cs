using Microsoft.AspNetCore.Mvc;
using Npgsql;
using hr_crm.Models;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TodoController(IConfiguration config)
        {
            _config = config;
        }

        // =====================================
        // GET: api/todo
        // =====================================
        [HttpGet]
        public IActionResult GetTasks()
        {
            var connStr = _config.GetConnectionString("HR_CRM");
            var tasks = new List<object>();

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                SELECT
                    t.task_id,
                    t.title,
                    t.description,
                    e.first_name,
                    t.due_date,
                    t.status,
                    t.created_at
                FROM todo_tasks t
                JOIN employees e
                    ON t.assigned_to = e.employee_id
                ORDER BY t.due_date;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(new
                {
                    TaskId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    AssignedTo = reader.GetString(3),
                    DueDate = reader.GetDateTime(4),
                    Status = reader.GetString(5),
                    CreatedAt = reader.GetDateTime(6)
                });
            }

            return Ok(tasks);
        }

        // =====================================
        // POST: api/todo
        // =====================================
        [HttpPost]
        public IActionResult AddTask([FromBody] TodoCreateDto task)
        {
            var connStr = _config.GetConnectionString("HR_CRM");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                INSERT INTO todo_tasks
                (title, description, assigned_to, due_date, status)
                VALUES
                (@title, @desc, @assigned, @due, @status);
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("title", task.Title);
            cmd.Parameters.AddWithValue("desc", task.Description);
            cmd.Parameters.AddWithValue("assigned", task.AssignedTo);
            cmd.Parameters.AddWithValue("due", task.DueDate);
            cmd.Parameters.AddWithValue("status", task.Status);

            cmd.ExecuteNonQuery();

            return Ok("To-Do task added successfully");
        }
    }
}

