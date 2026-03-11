using hr_crm.Models;
using hr_crm.Entities;
using Microsoft.AspNetCore.Mvc;
using hr_crm.Service.Interface;
using hr_crm.Service;
using System.Security.Claims;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _service;
        private readonly NotificationService _notificationService;

        public TodoController(ITodoService service, NotificationService notificationService)
        {
            _service = service;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
                return Unauthorized();

            var tokenUserId = int.Parse(userClaim.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var tasks = await _service.GetAllAsync();

            // HR_MANAGER → see all tasks
            if (role == "HR_MANAGER")
            {
                var allTasks = tasks.Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    t.Description,
                    AssignedTo = t.AssignedTo,
                    DueDate = t.DueDate.ToString("yyyy-MM-dd"),
                    t.Status,
                    t.CreatedAt
                });

                return Ok(allTasks);
            }

            // HR_USER → see only their tasks
            var userTasks = tasks
                .Where(t => t.AssignedTo == tokenUserId)
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    t.Description,
                    AssignedTo = t.AssignedTo,
                    DueDate = t.DueDate.ToString("yyyy-MM-dd"),
                    t.Status,
                    t.CreatedAt
                });

            return Ok(userTasks);
        }

        [HttpPost]
        public async Task<IActionResult> AddTask([FromBody] TodoCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request");

            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
                return Unauthorized();

            var tokenUserId = int.Parse(userClaim.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // HR_USER cannot assign to other users
            if (role == "HR_USER" && dto.AssignedTo != tokenUserId)
                return Forbid("You cannot assign tasks to another user.");

            var task = new TodoTask
            {
                Title = dto.Title,
                Description = dto.Description,
                AssignedTo = dto.AssignedTo,
                DueDate = DateOnly.FromDateTime(dto.DueDate),
                Status = dto.Status
            };

            await _service.CreateAsync(task);

            await _notificationService.CreateNotification(
                dto.AssignedTo,
                "New Task Assigned",
                "A new task has been assigned to you",
                "Todo",
                0
            );

            return Ok("To-Do task added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TodoCreateDto dto)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
                return Unauthorized();

            var tokenUserId = int.Parse(userClaim.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // HR_USER cannot update another user's task
            if (role == "HR_USER" && dto.AssignedTo != tokenUserId)
                return Forbid("You cannot update another user's task.");

            var task = new TodoTask
            {
                Title = dto.Title,
                Description = dto.Description,
                AssignedTo = dto.AssignedTo,
                DueDate = DateOnly.FromDateTime(dto.DueDate),
                Status = dto.Status
            };

            var result = await _service.UpdateAsync(id, task);

            if (!result)
                return NotFound("Task not found");

            return Ok("To-Do task updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound("Task not found");

            return Ok("Task deleted successfully");
        }
    }
}