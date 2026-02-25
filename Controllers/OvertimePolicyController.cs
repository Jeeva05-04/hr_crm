using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using hr_crm.Data;
using hr_crm.Authorization;
using hr_crm.Entities;
using hr_crm.DTO.Overtime;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OvertimePolicyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OvertimePolicyController(AppDbContext context)
        {
            _context = context;
        }

        // ===============================
        // ✅ CREATE POLICY (HR only)
        // ===============================
        [Authorize]
        [HasPermission("Overtime.Policy.Create")]
        [HttpPost]
        public async Task<IActionResult> CreatePolicy([FromBody] OvertimePolicyCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Prevent duplicate policy per department
            var existingPolicy = await _context.OvertimePolicies
                .FirstOrDefaultAsync(p => p.DepartmentId == dto.DepartmentId);

            if (existingPolicy != null)
                return BadRequest("Policy already exists for this department.");

            var policy = new OvertimePolicy
            {
                DepartmentId = dto.DepartmentId,
                StandardDailyHours = dto.StandardDailyHours,
                MaxWeeklyOvertimeHours = dto.MaxWeeklyOvertimeHours
            };

            _context.OvertimePolicies.Add(policy);
            await _context.SaveChangesAsync();

            return Ok("Overtime policy created successfully");
        }

        // ===============================
        // ✅ GET ALL POLICIES
        // ===============================
        [Authorize]
        [HasPermission("Overtime.Policy.View")]
        [HttpGet]
        public async Task<IActionResult> GetPolicies()
        {
            var policies = await _context.OvertimePolicies
                .Include(p => p.Department)
                .Select(p => new OvertimePolicyResponseDto
                {
                    OvertimePolicyId = p.OvertimePolicyId,
                    DepartmentId = p.DepartmentId,
                    StandardDailyHours = p.StandardDailyHours,
                    MaxWeeklyOvertimeHours = p.MaxWeeklyOvertimeHours
                })
                .ToListAsync();

            return Ok(policies);
        }

        // ===============================
        // ✅ UPDATE POLICY
        // ===============================
        [Authorize]
        [HasPermission("Overtime.Policy.Create")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePolicy(int id, [FromBody] OvertimePolicyUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var policy = await _context.OvertimePolicies.FindAsync(id);

            if (policy == null)
                return NotFound("Policy not found.");

            policy.StandardDailyHours = dto.StandardDailyHours;
            policy.MaxWeeklyOvertimeHours = dto.MaxWeeklyOvertimeHours;

            await _context.SaveChangesAsync();

            return Ok("Policy updated successfully.");
        }
    }
}