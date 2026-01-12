using hr_crm.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecruitmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public RecruitmentController(IConfiguration config)
        {
            _config = config;
        }

        // ✅ GET: api/recruitment
        [HttpGet]
        public IActionResult GetCandidates()
        {
            var connStr = _config.GetConnectionString("HR_CRM");
            var candidates = new List<object>();

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                SELECT
                    candidate_id,
                    first_name,
                    last_name,
                    email,
                    phone,
                    applied_position,
                    department_id,
                    application_date,
                    status,
                    source
                FROM recruitment
                ORDER BY application_date DESC;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                candidates.Add(new
                {
                    CandidateId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    Phone = reader.GetString(4),
                    AppliedPosition = reader.GetString(5),
                    DepartmentId = reader.GetInt32(6),
                    ApplicationDate = reader.GetDateTime(7),
                    Status = reader.GetString(8),
                    Source = reader.GetString(9)
                });
            }

            return Ok(candidates);
        }

      

[HttpPost]
    public IActionResult AddCandidate([FromBody] RecruitmentCreateDto candidate)
    {
        var connStr = _config.GetConnectionString("HR_CRM");

        if (string.IsNullOrEmpty(connStr))
            return StatusCode(500, "Connection string not found");

        using var conn = new NpgsqlConnection(connStr);
        conn.Open();

        var sql = @"
        INSERT INTO recruitment
        (first_name, last_name, email, phone, applied_position,
         department_id, application_date, status, source)
        VALUES
        (@fn, @ln, @em, @ph, @pos, @did, @adate, @status, @source);
    ";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("fn", candidate.FirstName);
        cmd.Parameters.AddWithValue("ln", candidate.LastName);
        cmd.Parameters.AddWithValue("em", candidate.Email);
        cmd.Parameters.AddWithValue("ph", candidate.Phone);
        cmd.Parameters.AddWithValue("pos", candidate.AppliedPosition);
        cmd.Parameters.AddWithValue("did", candidate.DepartmentId);
        cmd.Parameters.AddWithValue("adate", candidate.ApplicationDate);
        cmd.Parameters.AddWithValue("status", candidate.Status);
        cmd.Parameters.AddWithValue("source", candidate.Source);

        cmd.ExecuteNonQuery();

        return Ok("Candidate application added successfully");
    }

}
}

