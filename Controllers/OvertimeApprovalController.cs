using hr_crm.Authorization;
using hr_crm.Data;
using hr_crm.DTO.Overtime;
using hr_crm.Entities;
using hr_crm.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OvertimeApprovalController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public OvertimeApprovalController(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [Authorize]
        [HasPermission("OVERTIME_APPROVE")]
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

            await _notificationService.CreateNotification(
                dto.UserId,
                "Overtime Approved",
                "Your overtime request has been approved",
                "Overtime",
                approval.OvertimeApprovalId
            );

            return Ok(new OvertimeApprovalResponseDto
            {
                OvertimeApprovalId = approval.OvertimeApprovalId,
                UserId = approval.UserId,
                ValidFrom = approval.ValidFrom,
                ValidTo = approval.ValidTo,
                IsApproved = approval.IsApproved
            });
        }
    }
}