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
        [HasPermission("Shift.View")]
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
        // ✅ GET SHIFT BY ID
        // =====================================
        [Authorize]
        [HasPermission("Shift.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShiftById(int id)
        {
            var shift = await _context.Shifts
                .Where(s => s.ShiftId == id)
                .Select(s => new
                {
                    s.ShiftId,
                    s.ShiftName,
                    s.StartTime,
                    s.EndTime,
                    s.DepartmentId
                })
                .FirstOrDefaultAsync();

            if (shift == null)
                return NotFound("Shift not found");

            return Ok(shift);
        }

        // =====================================
        // ✅ GET USER ASSIGNED SHIFT
        // =====================================
        [Authorize]
        [HasPermission("Shift.View")]
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
        [HasPermission("Shift.Create")]
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
        [HasPermission("Shift.Assign")]
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