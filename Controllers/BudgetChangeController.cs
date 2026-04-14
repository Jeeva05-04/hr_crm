using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Authorization;
using hr_crm.Repositories.Interface;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetChangeController : ControllerBase
    {
        private readonly IBudgetChangeRequestRepository _repository;

        public BudgetChangeController(IBudgetChangeRequestRepository repository)
        {
            _repository = repository;
        }

        // ======================================
        // ✅ REQUEST BUDGET CHANGE
        // ======================================
        [Authorize]
        [HasPermission("BUDGET_REQUEST")]
        [HttpPost]
        public async Task<IActionResult> RequestBudgetChange([FromBody] BudgetRequestDto dto)
        {
            var request = new BudgetChangeRequest
            {
                DepartmentId = dto.DepartmentId,
                RequestedAmount = dto.RequestedAmount,
                Reason = dto.Reason
            };

            var created = await _repository.CreateAsync(request);

            return Ok(created);
        }

        // ======================================
        // ✅ GET ALL REQUESTS
        // ======================================
        [Authorize]
        [HasPermission("BUDGET_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _repository.GetAllAsync();
            return Ok(requests);
        }

        // ======================================
        // ✅ GET REQUEST BY ID
        // ======================================
        [Authorize]
        [HasPermission("BUDGET_VIEW")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _repository.GetByIdAsync(id);

            if (request == null)
                return NotFound("Request not found");

            return Ok(request);
        }

        // ======================================
        // ✅ APPROVE REQUEST
        // ======================================
        [Authorize]
        [HasPermission("BUDGET_APPROVE")]
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User ID not found");

            int userId = int.Parse(userIdClaim.Value);

            var result = await _repository.ApproveAsync(id, userId);

            if (!result)
                return BadRequest("Invalid request");

            return Ok("Budget change approved");
        }

        // ======================================
        // ❌ REJECT REQUEST
        // ======================================
        [Authorize]
        [HasPermission("BUDGET_APPROVE")]
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User ID not found");

            int userId = int.Parse(userIdClaim.Value);

            var result = await _repository.RejectAsync(id, userId);

            if (!result)
                return BadRequest("Invalid request");

            return Ok("Budget change rejected");
        }

        // ======================================
        // 🗑 DELETE REQUEST
        // ======================================
        [Authorize]
        [HasPermission("BUDGET_DELETE")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repository.DeleteAsync(id);

            if (!result)
                return NotFound("Request not found");

            return Ok("Budget change request deleted successfully");
        }
    }
}