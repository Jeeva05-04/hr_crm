using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Entities;
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

            // 1️⃣ HR Assign Training
          
            [HttpPost("assign")]
           [HasPermission("EmployeeTraining_assign")]
            public async Task<IActionResult> AssignTraining(AssignTrainingDto dto)
            {
                var result = await _service.AssignTrainingAsync(dto);
                return Ok(result);
            }

            // 2️⃣ Get Employee Trainings
            
            [HttpGet("employee/{employeeId}")]
            [HasPermission("EmployeeTraining_employee/{employeeId}")]   
            public async Task<IActionResult> GetByEmployee(int employeeId)
            {
                var result = await _service.GetByEmployeeAsync(employeeId);
                return Ok(result);
            }
  
        [HttpGet("all")]
        [HasPermission("EmployeeTraining_View")]
        public async Task<IActionResult> GetAllTrainings()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // 3️⃣ Update Status
     
            [HttpPut("update-status/{id}")]
        [HasPermission("EmployeeTraining_update-status/{id}")]
            public async Task<IActionResult> UpdateStatus(int id, UpdateTrainingStatusCreateDto dto)
            {
                var success = await _service.UpdateStatusAsync(id, dto);
                if (!success) return NotFound();
                return Ok("Status updated successfully");
            }

            // 4️⃣ Delete Training (HR Only)
         
            [HttpDelete("delete/{id}")]
            [HasPermission("EmployeeTraining_Delete")] 
        public async Task<IActionResult> Delete(int id)
            {
                var success = await _service.DeleteAsync(id);
                if (!success) return NotFound();
                return Ok("Deleted successfully");
            }
        
    }

}


