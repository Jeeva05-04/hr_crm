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
    public class OvertimeApprovalController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OvertimeApprovalController(AppDbContext context)
        {
            _context = context;
        }

        // ===============================
        // ✅ APPROVE OVERTIME
        // ===============================
        [Authorize]
        [HasPermission("Overtime.Approve")]
        [HttpPost]
        public async Task<IActionResult> ApproveOvertime([FromBody] OvertimeApprovalCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.ValidFrom > dto.ValidTo)
                return BadRequest("ValidFrom cannot be later than ValidTo.");

            var approval = new OvertimeApproval
            {
                UserId = dto.UserId,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                IsApproved = true
            };

            _context.OvertimeApprovals.Add(approval);
            await _context.SaveChangesAsync();

            return Ok(new OvertimeApprovalResponseDto
            {
                OvertimeApprovalId = approval.OvertimeApprovalId,
                UserId = approval.UserId,
                ValidFrom = approval.ValidFrom,
                ValidTo = approval.ValidTo,
                IsApproved = approval.IsApproved
            });
        }

        // ===============================
        // ✅ VIEW APPROVALS
        // ===============================
        [Authorize]
        [HasPermission("Overtime.Approve")]
        [HttpGet]
        public async Task<IActionResult> GetApprovals()
        {
            var approvals = await _context.OvertimeApprovals
                .Select(a => new OvertimeApprovalResponseDto
                {
                    OvertimeApprovalId = a.OvertimeApprovalId,
                    UserId = a.UserId,
                    ValidFrom = a.ValidFrom,
                    ValidTo = a.ValidTo,
                    IsApproved = a.IsApproved
                })
                .ToListAsync();

            return Ok(approvals);
        }

        // ===============================
        // ✅ UPDATE APPROVAL
        // ===============================
        [Authorize]
        [HasPermission("Overtime.Approve")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApproval(int id, [FromBody] OvertimeApprovalUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.ValidFrom > dto.ValidTo)
                return BadRequest("ValidFrom cannot be later than ValidTo.");

            var approval = await _context.OvertimeApprovals.FindAsync(id);

            if (approval == null)
                return NotFound("Approval not found.");

            approval.ValidFrom = dto.ValidFrom;
            approval.ValidTo = dto.ValidTo;
            approval.IsApproved = dto.IsApproved;

            await _context.SaveChangesAsync();

            return Ok("Approval updated successfully.");
        }
    }
}