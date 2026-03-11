using System.Security.Claims;
using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeTrainingController : ControllerBase
    {
        private readonly IEmployeeTrainingService _service;

        public EmployeeTrainingController(IEmployeeTrainingService service)
        {
            _service = service;
        }

        [HttpPost("assign")]
        [HasPermission("EMPLOYEETRAINING_ASSIGN")]
        public async Task<IActionResult> AssignTraining([FromBody] AssignTrainingDto dto)
        {
            var result = await _service.AssignTrainingAsync(dto);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [HasPermission("EMPLOYEETRAINING_VIEW")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var currentUserIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("id")?.Value ??
                User.FindFirst("userId")?.Value ??
                User.FindFirst("sub")?.Value;

            var currentUserRole =
                User.FindFirst(ClaimTypes.Role)?.Value ??
                User.FindFirst("role")?.Value ??
                User.FindFirst("roles")?.Value;

            if (string.IsNullOrEmpty(currentUserIdClaim))
                return Unauthorized("User ID not found in token");

            if (!int.TryParse(currentUserIdClaim, out int currentUserId))
                return Unauthorized("Invalid User ID in token");

            currentUserRole = currentUserRole?.ToUpper();

            if ((currentUserRole == "EMPLOYEE" || currentUserRole == "USER") && currentUserId != userId)
                return Forbid();

            var result = await _service.GetByUserAsync(userId);
            return Ok(result);
        }

        [HttpGet("all")]
        [HasPermission("EMPLOYEETRAINING_VIEW")]
        public async Task<IActionResult> GetAllTrainings()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpPut("update-status/{id}")]
        [HasPermission("EMPLOYEETRAINING_ASSIGN")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTrainingStatusCreateDto dto)
        {
            var success = await _service.UpdateStatusAsync(id, dto);

            if (!success)
                return NotFound("Training record not found");

            return Ok("Status updated successfully");
        }

        [HttpDelete("delete/{id}")]
        [HasPermission("EMPLOYEETRAINING_DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                return NotFound("Training record not found");

            return Ok("Deleted successfully");
        }
    }
}