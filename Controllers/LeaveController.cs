using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
            private readonly ILeaveService _service;

            public LeaveController(ILeaveService service)
            {
                _service = service;
            }

            [HttpPost("apply")]
        [HasPermission("Leave_Apply")]
            public async Task<IActionResult> ApplyLeave([FromBody] LeaveCreateDto dto)
            {
                await _service.ApplyLeaveAsync(dto);
                return Ok("Leave applied successfully");
            }

            [HttpGet]
        [HasPermission("Leave_View")]
            public async Task<IActionResult> GetAllLeaves()
            {
                var data = await _service.GetAllLeavesAsync();
                return Ok(data);
            }

            [HttpGet("{employeeId}")]
            [HasPermission("Leave_employeeId")]
        public async Task<IActionResult> GetLeavesByEmployee(int employeeId)
            {
                var data = await _service.GetLeavesByEmployeeAsync(employeeId);
                return Ok(data);
            }

            [HttpPut("{leaveId}/status")]
            [HasPermission("Leave_update-status/{leaveId}")]    
        public async Task<IActionResult> UpdateStatus(int leaveId, [FromBody] LeaveStatusDto dto)
            {
                var result = await _service.UpdateLeaveStatusAsync(leaveId, dto);
                if (!result) return NotFound("Leave not found");
                return Ok("Leave status updated successfully");
            }

            [HttpDelete("{leaveId}")]
            [HasPermission("Leave_Delete")]   
        public async Task<IActionResult> DeleteLeave(int leaveId)
            {
                var result = await _service.DeleteLeaveAsync(leaveId);
                if (!result) return NotFound("Leave not found");
                return Ok("Leave deleted successfully");
            }
        
    }
}

