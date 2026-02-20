using hr_crm.Models;
using hr_crm.Entities;
using Microsoft.AspNetCore.Mvc;
using hr_crm.Service.Interface;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _service;

        public TodoController(ITodoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _service.GetAllAsync();

            var result = tasks.Select(t => new
            {
                t.TaskId,
                t.Title,
                t.Description,
                AssignedTo = t.AssignedTo, // Just return ID
                DueDate = t.DueDate.ToString("yyyy-MM-dd"),
                t.Status,
                t.CreatedAt
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddTask([FromBody] TodoCreateDto dto)
        {
            var task = new TodoTask
            {
                Title = dto.Title,
                Description = dto.Description,
                AssignedTo = dto.AssignedTo,
                DueDate = DateOnly.FromDateTime(dto.DueDate),
                Status = dto.Status
            };

            await _service.CreateAsync(task);

            return Ok("To-Do task added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TodoCreateDto dto)
        {
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
