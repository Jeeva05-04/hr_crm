using hr_crm.Models;
using hr_crm.Entities;
using hr_crm.Services;
using Microsoft.AspNetCore.Mvc;

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

            return Ok("Candidate application added successfully");
        }
    }
}
