using Microsoft.AspNetCore.Mvc;
using Npgsql;
using hr_crm.Models;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KnowledgeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public KnowledgeController(IConfiguration config)
        {
            _config = config;
        }

        // ==================================================
        // GET: api/knowledge
        // ==================================================
        [HttpGet]
        public IActionResult GetKnowledge()
        {
            var connStr = _config.GetConnectionString("HR_CRM");
            var result = new List<object>();

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                SELECT 
                    branch_id,
                    record_type,
                    code,
                    title,
                    category,
                    sub_category,
                    summary,
                    approval_status,
                    apporved_by,
                    visibility,
                    status,
                    created_by,
                    created_date
                FROM knowledge
                ORDER BY created_date DESC;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new
                {
                    BranchId = reader.GetInt32(0),
                    RecordType = reader.GetString(1),
                    Code = reader.GetString(2),
                    Title = reader.GetString(3),
                    Category = reader.GetString(4),
                    SubCategory = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Summary = reader.GetString(6),
                    ApprovalStatus = reader.GetString(7),
                    ApprovedBy = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    Visibility = reader.GetString(9),
                    Status = reader.GetString(10),
                    CreatedBy = reader.GetInt32(11),
                    CreatedDate = reader.GetDateTime(12)
                });
            }

            return Ok(result);
        }

        // ==================================================
        // POST: api/knowledge
        // ==================================================
        [HttpPost]
        [Consumes("application/json")]
        public IActionResult AddKnowledge([FromBody] KnowledgeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var connStr = _config.GetConnectionString("HR_CRM");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var sql = @"
                INSERT INTO knowledge
                (branch_id, record_type, code, title, category, sub_category,
                 summary, approval_status, apporved_by, visibility, status, created_by)
                VALUES
                (@branchId, @recordType, @code, @title, @category, @subCategory,
                 @summary, @approvalStatus, @approvedBy, @visibility, @status, @createdBy);
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("branchId", dto.BranchId);
            cmd.Parameters.AddWithValue("recordType", dto.RecordType);
            cmd.Parameters.AddWithValue("code", dto.Code);
            cmd.Parameters.AddWithValue("title", dto.Title);
            cmd.Parameters.AddWithValue("category", dto.Category);
            cmd.Parameters.AddWithValue("subCategory", dto.SubCategory ?? "");
            cmd.Parameters.AddWithValue("summary", dto.Summary);
            cmd.Parameters.AddWithValue("approvalStatus", dto.ApprovalStatus);
            cmd.Parameters.AddWithValue("approvedBy", dto.ApprovedBy ?? "");
            cmd.Parameters.AddWithValue("visibility", dto.Visibility);
            cmd.Parameters.AddWithValue("status", dto.Status);
            cmd.Parameters.AddWithValue("createdBy", dto.CreatedBy);

            cmd.ExecuteNonQuery();

            return Ok("Knowledge record added successfully");
        }
    }
}

