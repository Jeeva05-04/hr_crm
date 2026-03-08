using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using hr_crm.Authorization;
using hr_crm.Data;
using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShiftController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HasPermission("SHIFT_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetAllShifts()
        {
            var shifts = await _context.Shifts
                .Select(s => new
                {
                    s.ShiftId,
                    s.ShiftName,
                    s.StartTime,
                    s.EndTime,
                    s.DepartmentId
                })
                .ToListAsync();

            return Ok(shifts);
        }

        [Authorize]
        [HasPermission("SHIFT_VIEW")]
        [HttpGet("assigned-users")]
        public async Task<IActionResult> GetAllAssignedUsers()
        {
            var data = await _context.UserShifts
                .Include(us => us.Shift)
                .Select(us => new
                {
                    us.UserId,
                    us.Shift.ShiftId,
                    us.Shift.ShiftName,
                    us.Shift.StartTime,
                    us.Shift.EndTime
                })
                .ToListAsync();

            if (!data.Any())
                return NotFound("No shift assignments found");

            return Ok(data);
        }

        [Authorize]
        [HasPermission("SHIFT_VIEW")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserShift(int userId)
        {
            var tokenUserId = int.Parse(User.FindFirst("sub")!.Value);

            if (userId != tokenUserId)
                return Forbid("You can only view your own shift.");

            var userShift = await _context.UserShifts
                .Include(us => us.Shift)
                .Where(us => us.UserId == userId)
                .Select(us => new
                {
                    us.UserId,
                    us.Shift.ShiftId,
                    us.Shift.ShiftName,
                    us.Shift.StartTime,
                    us.Shift.EndTime
                })
                .FirstOrDefaultAsync();

            if (userShift == null)
                return NotFound("Shift not assigned");

            return Ok(userShift);
        }

        [Authorize]
        [HasPermission("SHIFT_CREATE")]
        [HttpPost]
        public async Task<IActionResult> CreateShift(ShiftCreateDto dto)
        {
            var shift = new Shift
            {
                ShiftName = dto.ShiftName,
                StartTime = TimeSpan.Parse(dto.StartTime),
                EndTime = TimeSpan.Parse(dto.EndTime),
                DepartmentId = dto.DepartmentId
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return Ok("Shift created successfully");
        }

        [Authorize]
        [HasPermission("SHIFT_ASSIGN")]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignShift(int userId, int shiftId)
        {
            var existing = await _context.UserShifts
                .FirstOrDefaultAsync(us => us.UserId == userId);

            if (existing != null)
            {
                existing.ShiftId = shiftId;
            }
            else
            {
                _context.UserShifts.Add(new UserShift
                {
                    UserId = userId,
                    ShiftId = shiftId
                });
            }

            await _context.SaveChangesAsync();

            return Ok("Shift assigned successfully");
        }
    }
}