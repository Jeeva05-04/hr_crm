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

            var created = await _service.CreateAsync(budget);

            return Ok(created);
        }

        // ✅ GET ALL BUDGETS
        [HttpGet]
        public async Task<IActionResult> GetAllBudgets()
        {
            var budgets = await _service.GetAllAsync();
            return Ok(budgets);
        }

        // ✅ GET BUDGET BY DEPARTMENT
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            var budgets = await _service.GetByDepartmentIdAsync(departmentId);
            return Ok(budgets);
        }

        // ✅ GET BUDGET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var budget = await _service.GetByIdAsync(id);

            if (budget == null)
                return NotFound("Budget not found");

            return Ok(budget);
        }
    }
}