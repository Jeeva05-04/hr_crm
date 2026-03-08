using hr_crm.Models;
using hr_crm.Entities;
using Microsoft.AspNetCore.Mvc;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _service;

        public ProjectController(IProjectService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _service.GetAllAsync();

            var result = projects.Select(p => new
            {
                p.ProjectId,
                p.ProjectName,
                p.Duration,
                p.Status,
                ManagerId = p.ManagerId, // Just return ID
                DepartmentName = p.Department.DepartmentName,
                p.CreatedDate
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddProject([FromBody] ProjectCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = new Project
            {
                ProjectName = dto.ProjectName,
                Duration = dto.Duration,
                Status = dto.Status,
                ManagerId = dto.ManagerId,
                DepartmentId = dto.DepartmentId
            };

            await _service.CreateAsync(project);

            return Ok("Project created successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectCreateDto dto)
        {
            var project = new Project
            {
                ProjectName = dto.ProjectName,
                Duration = dto.Duration,
                Status = dto.Status,
                ManagerId = dto.ManagerId,
                DepartmentId = dto.DepartmentId
            };

            var result = await _service.UpdateAsync(id, project);

            if (!result)
                return NotFound("Project not found");

            return Ok("Project updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound("Project not found");

            return Ok("Project deleted successfully");
        }
    }
}