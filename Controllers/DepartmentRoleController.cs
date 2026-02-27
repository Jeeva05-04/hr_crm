using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using hr_crm.Authorization;
using hr_crm.Data;
using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentRoleController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartmentRoleController(AppDbContext context)
    {
        _context = context;
    }

    // =====================================
    // ✅ CREATE ROLE (HR only)
    // =====================================
    [Authorize]
    [HasPermission("ROLE_CREATE")]
    [HttpPost]
    public async Task<IActionResult> CreateRole(DepartmentRoleCreateDto dto)
    {
        var role = new DepartmentRole
        {
            RoleName = dto.RoleName,
            RequiredSkillLevel = dto.RequiredSkillLevel,
            PerformanceLevel = dto.PerformanceLevel,
            DepartmentId = dto.DepartmentId
        };

        _context.DepartmentRoles.Add(role);
        await _context.SaveChangesAsync();

        return Ok("Role created successfully");
    }

    // =====================================
    // ✅ GET ALL ROLES
    // =====================================
    [Authorize]
    [HasPermission("ROLE_VIEW")]
    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _context.DepartmentRoles
            .Include(r => r.Department)
            .Select(r => new
            {
                r.DepartmentRoleId,
                r.RoleName,
                r.RequiredSkillLevel,
                r.PerformanceLevel,
                r.DepartmentId,
                DepartmentName = r.Department.DepartmentName
            })
            .ToListAsync();

        return Ok(roles);
    }

    // =====================================
    // ✅ ASSIGN ROLE TO USER
    // =====================================
    [Authorize]
    [HasPermission("ROLE_ASSIGN")]
    [HttpPost("assign")]
    public async Task<IActionResult> AssignRoleToUser(AssignUserRoleDto dto)
    {
        var existing = await _context.UserDepartmentRoles
            .FirstOrDefaultAsync(x => x.UserId == dto.UserId);

        if (existing != null)
        {
            existing.DepartmentRoleId = dto.DepartmentRoleId;
        }
        else
        {
            _context.UserDepartmentRoles.Add(new UserDepartmentRole
            {
                UserId = dto.UserId,
                DepartmentRoleId = dto.DepartmentRoleId
            });
        }

        await _context.SaveChangesAsync();

        return Ok("Role assigned successfully");
    }

    // =====================================
    // ✅ GET USER ROLE
    // =====================================
    [Authorize]
    [HasPermission("ROLE_VIEW")]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserRole(int userId)
    {
        var userRole = await _context.UserDepartmentRoles
            .Include(ur => ur.DepartmentRole)
            .ThenInclude(r => r.Department)
            .Where(ur => ur.UserId == userId)
            .Select(ur => new
            {
                ur.UserId,
                ur.DepartmentRole.RoleName,
                ur.DepartmentRole.RequiredSkillLevel,
                ur.DepartmentRole.PerformanceLevel,
                Department = ur.DepartmentRole.Department.DepartmentName
            })
            .FirstOrDefaultAsync();

        if (userRole == null)
            return NotFound("No role assigned");

        return Ok(userRole);
    }

    // =====================================
    // ✅ UPDATE ROLE
    // =====================================
    [Authorize]
    [HasPermission("ROLE_UPDATE")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, DepartmentRoleCreateDto dto)
    {
        var role = await _context.DepartmentRoles.FindAsync(id);

        if (role == null)
            return NotFound("Role not found");

        role.RoleName = dto.RoleName;
        role.RequiredSkillLevel = dto.RequiredSkillLevel;
        role.PerformanceLevel = dto.PerformanceLevel;
        role.DepartmentId = dto.DepartmentId;

        await _context.SaveChangesAsync();

        return Ok("Role updated successfully");
    }

    // =====================================
    // ✅ DELETE ROLE
    // =====================================
    [Authorize]
    [HasPermission("ROLE_DELETE")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.DepartmentRoles
            .Include(r => r.UserDepartmentRoles)
            .FirstOrDefaultAsync(r => r.DepartmentRoleId == id);

        if (role == null)
            return NotFound("Role not found");

        // 🔥 Prevent deleting if assigned to users
        if (role.UserDepartmentRoles.Any())
            return BadRequest("Cannot delete role assigned to users");

        _context.DepartmentRoles.Remove(role);
        await _context.SaveChangesAsync();

        return Ok("Role deleted successfully");
    }
}