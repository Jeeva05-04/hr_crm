using hr_crm.Authorization;
using hr_crm.DTO;
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
        [HasPermission("EXITINTERVIEW_SCHEDULE")]
        public async Task<IActionResult> ScheduleInterview(ExitInterviewRequestDto dto)
        {
            var result = await _service.ScheduleInterview(dto);
            return Ok(result);
        }

        [HttpPost("submit-feedback")]
        [Authorize(Roles = "HR_MANAGER,ADMIN,HR,MANAGER")]
        public async Task<IActionResult> SubmitFeedback(ExitInterviewFeedbackDto dto)
        {
            var result = await _service.SubmitFeedback(dto);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [HasPermission("EXITINTERVIEW_VIEW")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var result = await _service.GetByUserId(userId);
            return Ok(result);
        }

        [HttpGet("all")]
        [HasPermission("EXITINTERVIEW_VIEW")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        [Authorize(Roles = "HR_MANAGER,ADMIN,HR,MANAGER")]
        public async Task<IActionResult> UpdateInterview(int id, ExitInterviewResponseDto dto)
        {
            var result = await _service.UpdateInterview(id, dto);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "HR_MANAGER,ADMIN,HR,MANAGER")]
        public async Task<IActionResult> DeleteInterview(int id)
        {
            await _service.DeleteInterview(id);
            return Ok("Exit Interview Deleted Successfully");
        }
    }
}