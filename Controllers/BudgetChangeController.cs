using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hr_crm.Data;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Authorization;

namespace hr_crm.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetChangeController : ControllerBase
{
    private readonly AppDbContext _context;

    public BudgetChangeController(AppDbContext context)
    {
        _context = context;
    }

    // ======================================
    // ✅ REQUEST BUDGET CHANGE
    // ======================================
    [Authorize]
    [HasPermission("BUDGET_REQUEST")]
    [HttpPost("request")]
    public async Task<IActionResult> RequestBudgetChange(BudgetRequestDto dto)
    {
        var request = new BudgetChangeRequest
        {
            DepartmentId = dto.DepartmentId,
            RequestedAmount = dto.RequestedAmount,
            Reason = dto.Reason
        };

        _context.BudgetChangeRequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok("Budget change request submitted");
    }
}