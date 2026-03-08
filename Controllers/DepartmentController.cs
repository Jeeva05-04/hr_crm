using hr_crm.Models;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hr_crm.Authorization;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _service;

        public DepartmentController(IDepartmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _service.GetAllAsync();

            var result = departments.Select(d => new
            {
                d.DepartmentId,
                d.DepartmentName,
                d.BranchId,
                BranchName = d.Branch?.BranchName
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddDepartment([FromBody] DepartmentDto dto)
        {
            await _service.CreateAsync(dto.DepartmentName, dto.BranchId);
            return Ok("Department added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentDto dto)
        {
            var result = await _service.UpdateAsync(id, dto.DepartmentName, dto.BranchId);

            if (!result)
                return NotFound("Department not found");

            return Ok("Department updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound("Department not found");

            return Ok("Department deleted successfully");
        }

        [Authorize]
        [HasPermission("ROLE_VIEW")]
        [HttpGet("{departmentId}/users")]
        public async Task<IActionResult> GetUsersInDepartment(int departmentId)
        {
            var result = await _service.GetUsersInDepartmentAsync(departmentId);

            if (result == null)
                return NotFound("Department not found");

            return Ok(result);
        }
    }
}