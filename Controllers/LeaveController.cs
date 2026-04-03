using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using hr_crm.Extensions;
using hr_crm.Extensions;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _service;
        private readonly NotificationService _notification;
        private readonly LoggingService _loggingService;
        private readonly hr_crm.Data.AppDbContext _db;

        public LeaveController(ILeaveService service, NotificationService notification, LoggingService loggingService, hr_crm.Data.AppDbContext db)
        {
            _service = service;
            _notification = notification;
            _loggingService = loggingService;
            _db = db;
        }

        // =============================================
        // Leave Types (HR can manage)
        // =============================================
        [HttpPost("types")]
        [HasPermission("LEAVE_UPDATE")]
        public async Task<IActionResult> CreateLeaveType([FromBody] DTO.LeaveTypeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var createdBy = int.Parse(userIdClaim.Value);

            var exists = await _db.LeaveTypes.AnyAsync(t => t.Name.ToLower() == dto.Name.Trim().ToLower());
            if (exists) return Conflict(new { Message = "Leave type already exists." });

            var entity = new Entities.LeaveType
            {
                Name = dto.Name.Trim(),
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _db.LeaveTypes.Add(entity);
            await _db.SaveChangesAsync();

            try { await _loggingService.CreateLog(createdBy, User.GetDisplayName(), "LeaveTypeCreate", $"Name={entity.Name}"); } catch { }

            dto.LeaveTypeId = entity.LeaveTypeId;
            return Ok(dto);
        }

        [HttpGet("types")]
        [HasPermission("LEAVE_VIEW")]
        public async Task<IActionResult> GetLeaveTypes()
        {
            var types = await _db.LeaveTypes
                .OrderBy(t => t.Name)
                .Select(t => new DTO.LeaveTypeDto { LeaveTypeId = t.LeaveTypeId, Name = t.Name })
                .ToListAsync();

            return Ok(types);
        }

        [HttpPost("apply")]
        [HasPermission("LEAVE_APPLY")]
        public async Task<IActionResult> ApplyLeave([FromBody] LeaveCreateDto dto)
        {
            var (success, error) = await _service.ApplyLeaveAsync(dto);
            if (!success) return BadRequest(new { Message = error });

            // Find the newly created leave record (best-effort) to use as reference
            var leave = await _db.Leaves
                .Where(l => l.UserId == dto.UserId && l.StartDate == dto.StartDate && l.EndDate == dto.EndDate && l.Status == "Pending")
                .OrderByDescending(l => l.AppliedOn)
                .FirstOrDefaultAsync();

            // Notify all HR managers about the new leave request
            try
            {
                var hrRoleNames = new[] { "HR_MANAGER" };
                var hrUserIds = await _db.UserDepartmentRoles
                    .Include(ud => ud.DepartmentRole)
                    .Where(ud => hrRoleNames.Contains(ud.DepartmentRole.RoleName))
                    .Select(ud => ud.UserId)
                    .Distinct()
                    .ToListAsync();

                // Resolve applicant name from payroll snapshot or fallback to "Employee"
                var applicantName = "Employee";
                try
                {
                    var payrollSnapshot = await _db.Payrolls
                        .Where(p => p.UserId == dto.UserId && !string.IsNullOrEmpty(p.UserName))
                        .OrderByDescending(p => p.CreatedDate)
                        .FirstOrDefaultAsync();
                    if (payrollSnapshot != null)
                        applicantName = payrollSnapshot.UserName;
                }
                catch { }
                var message = $"Leave request from {applicantName} for {dto.StartDate:dd MMM yyyy} - {dto.EndDate:dd MMM yyyy} requires your approval.";

                foreach (var hrId in hrUserIds)
                {
                    // referenceId set to leave id if available
                    var referenceId = leave != null ? leave.LeaveId : 0;
                    await _notification.CreateNotification(hrId, "Leave Approval Required", message, "Leave", referenceId);
                }
            }
            catch { /* don't fail the apply flow because notifications failed */ }

            return Ok(new { Message = "Leave applied successfully. Pending manager approval." });
        }

        [HttpGet]
        [HasPermission("LEAVE_VIEW")]
        public async Task<IActionResult> GetAllLeaves()
            => Ok(await _service.GetAllLeavesAsync());

        [HttpGet("{userId}")]
        [HasPermission("LEAVE_VIEW")]
        public async Task<IActionResult> GetLeavesByUser(int userId)
            => Ok(await _service.GetLeavesByUserAsync(userId));

        [HttpPut("{leaveId}/status")]
        [HasPermission("LEAVE_UPDATE")]
        public async Task<IActionResult> UpdateStatus(int leaveId, [FromBody] LeaveStatusDto dto)
        {
            var (success, error) = await _service.UpdateLeaveStatusAsync(leaveId, dto);
            if (!success) return BadRequest(new { Message = error });

            var leaves = await _service.GetAllLeavesAsync();
            var leave = leaves.FirstOrDefault(l => l.LeaveId == leaveId);
            if (leave != null)
            {
                var label = dto.Status?.ToLower() == "approved" ? "Approved" : "Rejected";
                await _notification.CreateNotification(
                    leave.UserId,
                    $"Leave {label}",
                    $"Your leave ({leave.StartDate:dd MMM yyyy} – {leave.EndDate:dd MMM yyyy}) has been {label.ToLower()}.",
                    "Leave", leaveId);
            }

            return Ok(new { Message = $"Leave status updated to {dto.Status}." });
        }

        [HttpDelete("{leaveId}")]
        [HasPermission("LEAVE_DELETE")]
        public async Task<IActionResult> DeleteLeave(int leaveId)
        {
            var result = await _service.DeleteLeaveAsync(leaveId);
            if (!result) return NotFound(new { Message = "Leave not found." });
            return Ok(new { Message = "Leave deleted." });
        }

        // =============================================
        // Leave Balance
        // =============================================
        [HttpGet("balance/{userId}")]
        [HasPermission("LEAVE_VIEW")]
        public async Task<IActionResult> GetBalance(int userId)
        {
            var balance = await _service.GetBalanceAsync(userId);
            return Ok(new
            {
                UserId = userId,
                Year = DateTime.UtcNow.Year,
                Balance = balance.Select(b => new
                {
                    b.LeaveType,
                    b.TotalAllowed,
                    b.UsedDays,
                    b.RemainingDays
                })
            });
        }

        // =============================================
        // Holiday Master
        // =============================================
        [HttpPost("holiday")]
        [HasPermission("LEAVE_UPDATE")]
        public async Task<IActionResult> AddHoliday([FromBody] HolidayCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var result = await _service.AddHolidayAsync(dto, int.Parse(userIdClaim.Value));
            return Ok(new { Message = "Holiday added.", Holiday = result });
        }

        [HttpGet("holidays")]
        [HasPermission("LEAVE_VIEW")]
        public async Task<IActionResult> GetHolidays([FromQuery] int? year)
            => Ok(await _service.GetHolidaysAsync(year ?? DateTime.UtcNow.Year));

        [HttpDelete("holiday/{id}")]
        [HasPermission("LEAVE_UPDATE")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var result = await _service.DeleteHolidayAsync(id);
            if (!result) return NotFound(new { Message = "Holiday not found." });
            return Ok(new { Message = "Holiday deleted." });
        }

        // =============================================
        // Leave Calendar
        // =============================================
        [HttpGet("calendar")]
        [HasPermission("LEAVE_VIEW")]
        public async Task<IActionResult> GetCalendar([FromQuery] int? month, [FromQuery] int? year)
        {
            var result = await _service.GetCalendarAsync(
                month ?? DateTime.UtcNow.Month,
                year ?? DateTime.UtcNow.Year);
            return Ok(result);
        }

        // =============================================
        // Leave Encashment
        // =============================================
        [HttpPost("encashment/{userId}")]
        [HasPermission("LEAVE_UPDATE")]
        public async Task<IActionResult> ProcessEncashment(int userId, [FromQuery] string userName, [FromQuery] int? year)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var (result, error) = await _service.ProcessEncashmentAsync(
                userId, userName, year ?? DateTime.UtcNow.Year, int.Parse(userIdClaim.Value));

            if (result == null) return BadRequest(new { Message = error });

            await _notification.CreateNotification(userId, "Leave Encashment Processed",
                $"Your {result.EncashedDays} unused Earned leave days for {result.Year} have been encashed. Amount: ₹{result.AmountPaid:N2}.",
                "Leave", result.Id);

            return Ok(new { Message = "Encashment processed.", result.EncashedDays, result.AmountPaid, result.Year });
        }

        [HttpGet("encashment/{userId}")]
        [HasPermission("LEAVE_VIEW")]
        public async Task<IActionResult> GetEncashments(int userId)
            => Ok(await _service.GetEncashmentsAsync(userId));
    }
}
