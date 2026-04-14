using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using hr_crm.Data;
using hr_crm.Authorization;
using hr_crm.DTO.Overtime;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OvertimeRecordController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OvertimeRecordController(AppDbContext context)
        {
            _context = context;
        }

        // ===============================
        // ✅ GET USER OVERTIME HISTORY
        // ===============================
        [Authorize]
        [HasPermission("OVERTIME_VIEW")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOvertime(int userId)
        {
            var tokenUserId = int.Parse(User.FindFirst("sub")!.Value);

            if (userId != tokenUserId)
                return Forbid("You can only view your own overtime.");

            var records = await _context.OvertimeRecords
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Date)
                .Select(o => new OvertimeRecordResponseDto
                {
                    OvertimeRecordId = o.OvertimeRecordId,
                    UserId = o.UserId,
                    Date = o.Date,
                    OvertimeHours = o.OvertimeHours
                })
                .ToListAsync();

            return Ok(records);
        }

        // ===============================
        // ✅ WEEKLY SUMMARY
        // ===============================
        [Authorize]
        [HasPermission("OVERTIME_VIEW")]
        [HttpGet("weekly/{userId}")]
        public async Task<IActionResult> GetWeeklyOvertime(int userId)
        {
            var tokenUserId = int.Parse(User.FindFirst("sub")!.Value);

            if (userId != tokenUserId)
                return Forbid("You can only view your own overtime.");

            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            var weeklyTotal = await _context.OvertimeRecords
                .Where(o => o.UserId == userId &&
                            o.Date >= weekStart &&
                            o.Date <= today)
                .SumAsync(o => o.OvertimeHours);

            return Ok(new
            {
                UserId = userId,
                WeeklyOvertimeHours = weeklyTotal
            });
        }
    }
}