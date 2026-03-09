
using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExitInterviewController : ControllerBase
    {
        private readonly IExitInterviewService _service;

        public ExitInterviewController(IExitInterviewService service)
        {
            _service = service;
        }

        [HttpPost("schedule")]
        [HasPermission("ExitInterview_Schedule")]
        public async Task<IActionResult> ScheduleInterview(ExitInterviewRequestDto dto)
        {
            var result = await _service.ScheduleInterview(dto);
            return Ok(result);
        }

        [HttpPost("submit-feedback")]
        [HasPermission("ExitInterview_SubmitFeedback")]
        public async Task<IActionResult> SubmitFeedback(ExitInterviewFeedbackDto dto)
        {
            var result = await _service.SubmitFeedback(dto);
            return Ok(result);
        }

        [HttpGet("employee/{employeeId}")]
         [HasPermission("ExitInterview_employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var result = await _service.GetByEmployeeId(employeeId);
            return Ok(result);
        }

        [HttpGet("all")]
        [HasPermission("ExitInterview_View")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }
        [HttpPut("update/{id}")]
        [HasPermission("ExitInterview_Update")]
        public async Task<IActionResult> UpdateInterview(int id, ExitInterviewResponseDto dto)
        {
            var result = await _service.UpdateInterview(id, dto);
            return Ok(result);
        }
        [HttpDelete("delete/{id}")]
        [HasPermission("ExitInterview_Delete")]
        public async Task<IActionResult> DeleteInterview(int id)
        {
            await _service.DeleteInterview(id);
            return Ok("Exit Interview Deleted Successfully");
        }
    }
}

