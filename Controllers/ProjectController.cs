using hr_crm.Models;
using hr_crm.Entities;
using Microsoft.AspNetCore.Mvc;
using hr_crm.Service.Interface;
using hr_crm.Service;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _service;
        private readonly NotificationService _notificationService;

        public ProjectController(IProjectService service, NotificationService notificationService)
        {
            _service = service;
            _notificationService = notificationService;
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
                ManagerId = p.ManagerId,
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

            // 🔔 Notification
            await _notificationService.CreateNotification(
                dto.ManagerId,
                "Project Assigned",
                $"You have been assigned to project {dto.ProjectName}",
                "Project",
                project.ProjectId
            );

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