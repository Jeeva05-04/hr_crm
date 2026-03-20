using hr_crm.DTO;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobOpeningController : ControllerBase
    {
        private readonly IJobOpeningService _service;

        public JobOpeningController(IJobOpeningService service)
        {
            _service = service;
        }

        // GET /api/jobopening
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // GET /api/jobopening/department/{departmentId}
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            var result = await _service.GetByDepartmentAsync(departmentId);
            return Ok(result);
        }

        // GET /api/jobopening/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result is null)
                return NotFound(new { Message = "Job opening not found." });
            return Ok(result);
        }

        // POST /api/jobopening  — HR creates a new job opening
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobOpeningCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(new { Message = "Job opening created successfully.", Data = result });
        }

        // PUT /api/jobopening/{id}  — update title, openings count, status (Open/Closed/Paused)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] JobOpeningUpdateDto dto)
        {
            var (success, error) = await _service.UpdateAsync(id, dto);
            if (!success)
                return BadRequest(new { Message = error });
            return Ok(new { Message = "Job opening updated successfully." });
        }

        // DELETE /api/jobopening/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { Message = error });
            return Ok(new { Message = "Job opening deleted successfully." });
        }
    }
}
