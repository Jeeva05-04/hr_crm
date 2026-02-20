using hr_crm.Entities;
using hr_crm.Models;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KnowledgeController : ControllerBase
    {
        private readonly IKnowledgeService _service;

        public KnowledgeController(IKnowledgeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetKnowledge()
        {
            var records = await _service.GetAllAsync();

            var result = records.Select(k => new
            {
                k.BranchId,
                k.RecordType,
                k.Code,
                k.Title,
                k.Category,
                k.SubCategory,
                k.Summary,
                k.ApprovalStatus,
                k.ApporvedBy,
                k.Visibility,
                k.Status,
                k.CreatedBy,
                k.CreatedDate
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddKnowledge([FromBody] KnowledgeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var knowledge = new Knowledge
            {
                BranchId = dto.BranchId,
                RecordType = dto.RecordType,
                Code = dto.Code,
                Title = dto.Title,
                Category = dto.Category,
                SubCategory = dto.SubCategory,
                Summary = dto.Summary,
                ApprovalStatus = dto.ApprovalStatus,
                ApporvedBy = dto.ApprovedBy,
                Visibility = dto.Visibility,
                Status = dto.Status,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.UtcNow
            };

            await _service.CreateAsync(knowledge);

            return Ok("Knowledge record added successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKnowledge(int id)
        {
            var result = await _service.DeactivateAsync(id);

            if (!result)
                return NotFound("Knowledge record not found");

            return Ok("Knowledge record deactivated successfully");
        }
    }
}
