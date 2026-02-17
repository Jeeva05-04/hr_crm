using hr_crm.Models;
using hr_crm.Entities;
using hr_crm.Services;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeeController(IEmployeeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _service.GetAllAsync();

            var result = employees.Select(e => new
            {
                e.EmployeeId,
                e.FirstName,
                e.Email,
                e.Phone,
                e.EmergencyContact,
                e.DepartmentId,
                e.Designation,
                e.Address,
                DateOfJoining = e.DateOfJoining.ToString("yyyy-MM-dd"),
                e.Salary,
                e.Status
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] EmployeeCreateDto dto)
        {
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                EmergencyContact = dto.EmergencyContact,
                DepartmentId = dto.DepartmentId,
                Designation = dto.Designation,
                Address = dto.Address,
                Salary = dto.Salary,
                Status = "Active",
                DateOfJoining = DateOnly.FromDateTime(DateTime.Today)
            };

            await _service.CreateAsync(employee);

            return Ok("Employee added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeCreateDto dto)
        {
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                EmergencyContact = dto.EmergencyContact,
                DepartmentId = dto.DepartmentId,
                Designation = dto.Designation,
                Address = dto.Address,
                Salary = dto.Salary
            };

            var result = await _service.UpdateAsync(id, employee);

            if (!result)
                return NotFound("Employee not found");

            return Ok("Employee updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var result = await _service.DeactivateAsync(id);

            if (!result)
                return NotFound("Employee not found");

            return Ok("Employee deactivated successfully");
        }
    }
}
