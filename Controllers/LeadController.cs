using hr_crm.DTO;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeadController : ControllerBase
    {
        private readonly ILeadService _service;

        public LeadController(ILeadService service)
        {
            _service = service;
        }

        // GET /api/lead
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var leads = await _service.GetAllAsync();
            return Ok(leads);
        }

        // GET /api/lead/status/{status}
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var leads = await _service.GetByStatusAsync(status);
            return Ok(leads);
        }

        // GET /api/lead/assigned/{userId}  — employee sees their own assigned leads
        [HttpGet("assigned/{userId}")]
        public async Task<IActionResult> GetByAssignedUser(int userId)
        {
            var leads = await _service.GetByAssignedUserAsync(userId);
            return Ok(leads);
        }

        // GET /api/lead/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lead = await _service.GetByIdAsync(id);
            if (lead is null)
                return NotFound(new { Message = "Lead not found." });

            return Ok(lead);
        }

        // POST /api/lead  — HR adds a new lead (from social media CRM)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LeadCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(new { Message = "Lead created successfully.", Data = result });
        }

        // PUT /api/lead/{id}/assign  — HR assigns lead to employee (notification sent automatically)
        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignLead(int id, [FromBody] LeadAssignDto dto)
        {
            var (success, error) = await _service.AssignLeadAsync(id, dto);
            if (!success)
                return BadRequest(new { Message = error });

            return Ok(new { Message = "Lead assigned successfully. Employee has been notified." });
        }

        // PUT /api/lead/{id}/status  — update lead status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] LeadUpdateStatusDto dto)
        {
            var (success, error) = await _service.UpdateStatusAsync(id, dto);
            if (!success)
                return BadRequest(new { Message = error });

            return Ok(new { Message = "Lead status updated successfully." });
        }

        // DELETE /api/lead/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { Message = error });

            return Ok(new { Message = "Lead deleted successfully." });
        }
    }
}
