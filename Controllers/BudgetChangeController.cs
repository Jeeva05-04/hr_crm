using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hr_crm.Entities;
using hr_crm.Service.Interface;
using hr_crm.Authorization;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetChangeController : ControllerBase
    {
        private readonly IBudgetChangeRequestService _service;

        public BudgetChangeController(IBudgetChangeRequestService service)
        {
            _service = service;
        }

        // ✅ CREATE REQUEST
        [Authorize]
        [HasPermission("BUDGET_REQUEST")]
        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] BudgetChangeRequest request)
        {
            request.Status = "Pending";
            request.RequestDate = DateTime.UtcNow;

            var created = await _service.CreateAsync(request);

            return Ok(created);
        }

        // ✅ GET ALL REQUESTS
        [Authorize]
        [HasPermission("BUDGET_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _service.GetAllAsync();
            return Ok(requests);
        }

        // ✅ GET BY ID
        [Authorize]
        [HasPermission("BUDGET_VIEW")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _service.GetByIdAsync(id);

            if (request == null)
                return NotFound("Request not found");

            return Ok(request);
        }
    }
}