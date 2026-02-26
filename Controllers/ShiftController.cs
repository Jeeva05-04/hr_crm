using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using hr_crm.Authorization;
using hr_crm.Entities;
using hr_crm.DTO;
using hr_crm.Data;

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

        // =====================================
        // ✅ GET ALL SHIFTS
        // =====================================
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

        // =====================================
        // ✅ GET ALL USERS WITH THEIR SHIFTS
        // =====================================
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

        // =====================================
        // ✅ GET USER ASSIGNED SHIFT
        // =====================================
        [Authorize]
        [HasPermission("SHIFT_VIEW")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserShift(int userId)
        {
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

        // =====================================
        // ✅ CREATE SHIFT
        // =====================================
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

            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type} : {claim.Value}");
            }

            return Ok("Shift created successfully");
        }

        // =====================================
        // ✅ ASSIGN SHIFT
        // =====================================
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