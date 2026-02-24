using hr_crm.Models;
using hr_crm.Entities;
using Microsoft.AspNetCore.Mvc;
using hr_crm.Service.Interface;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecruitmentController : ControllerBase
    {
        private readonly IRecruitmentService _service;

        public RecruitmentController(IRecruitmentService service)
        {
            _service = service;
        }

        // =========================================
        // ✅ GET ALL
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GetCandidates()
        {
            var candidates = await _service.GetAllAsync();

            var result = candidates.Select(c => new
            {
                c.CandidateId,
                c.FirstName,
                c.LastName,
                c.Email,
                c.Phone,
                c.AppliedPosition,
                c.DepartmentId,
                ApplicationDate = c.ApplicationDate.ToString("yyyy-MM-dd"),
                c.Status,
                c.Source
            });

            return Ok(result);
        }

        // =========================================
        // ✅ CREATE
        // =========================================
        [HttpPost]
        public async Task<IActionResult> AddCandidate([FromBody] RecruitmentCreateDto dto)
        {
            var recruitment = new Recruitment
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                AppliedPosition = dto.AppliedPosition,
                DepartmentId = dto.DepartmentId,
                ApplicationDate = DateOnly.FromDateTime(dto.ApplicationDate),
                Status = dto.Status,
                Source = dto.Source
            };

            await _service.CreateAsync(recruitment);

            return Ok(new { Message = "Candidate application added successfully" });
        }

        // =========================================
        // ✅ UPDATE
        // =========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCandidate(int id, [FromBody] RecruitmentCreateDto dto)
        {
            var existing = await _service.GetByIdAsync(id);

            if (existing == null)
                return NotFound("Candidate not found");

            existing.FirstName = dto.FirstName;
            existing.LastName = dto.LastName;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            existing.AppliedPosition = dto.AppliedPosition;
            existing.DepartmentId = dto.DepartmentId;
            existing.ApplicationDate = DateOnly.FromDateTime(dto.ApplicationDate);
            existing.Status = dto.Status;
            existing.Source = dto.Source;

            await _service.UpdateAsync(existing);

            return Ok(new { Message = "Candidate updated successfully" });
        }

        // =========================================
        // ✅ DELETE
        // =========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidate(int id)
        {
            var existing = await _service.GetByIdAsync(id);

            if (existing == null)
                return NotFound("Candidate not found");

            await _service.DeleteAsync(id);

            return Ok(new { Message = "Candidate deleted successfully" });
        }
    }
}