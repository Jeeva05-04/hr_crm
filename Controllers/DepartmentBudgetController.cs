using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Authorization;
using hr_crm.Service.Interface;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentBudgetController : ControllerBase
    {
        private readonly IDepartmentBudgetService _service;

        public DepartmentBudgetController(IDepartmentBudgetService service)
        {
            _service = service;
        }

        // ✅ CREATE BUDGET
        [Authorize]
        [HasPermission("BUDGET_CREATE")]
        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] DepartmentBudgetDto dto)
        {
            var budget = new DepartmentBudget
            {
                DepartmentId = dto.DepartmentId,
                TotalAnnualBudget = dto.TotalAnnualBudget,
                TrainingBudget = dto.TrainingBudget,
                ResourceBudget = dto.ResourceBudget,
                UsedBudget = 0,
                Year = dto.Year
            };

            await _service.CreateAsync(budget);

            return Ok("Budget created successfully");
        }
    }
}