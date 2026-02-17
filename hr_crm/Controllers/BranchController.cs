using hr_crm.Models;
using hr_crm.Services;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _service;

        public BranchController(IBranchService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _service.GetAllAsync();
            return Ok(branches);
        }

        [HttpPost]
        public async Task<IActionResult> AddBranch([FromBody] BranchCreateDto dto)
        {
            await _service.CreateAsync(dto.BranchName, dto.Location, dto.Status);
            return Ok("Branch added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBranch(int id, [FromBody] BranchCreateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto.BranchName, dto.Location, dto.Status);

            if (!result)
                return NotFound("Branch not found");

            return Ok("Branch updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var result = await _service.DeactivateAsync(id);

            if (!result)
                return NotFound("Branch not found");

            return Ok("Branch deactivated successfully");
        }
    }
}
