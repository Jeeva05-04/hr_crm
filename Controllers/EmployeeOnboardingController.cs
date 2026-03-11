using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using hr_crm.Data;
using hr_crm.Service.Interface;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Authorization;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeOnboardingController : ControllerBase
    {
        private readonly IEmployeeOnboardingService _service;

        public EmployeeOnboardingController(IEmployeeOnboardingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] EmployeeOnboardingCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        // DELETE EMPLOYEE ONBOARDING
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound("Onboarding record not found");

            return Ok("Employee onboarding deleted successfully");
        }
    }
}