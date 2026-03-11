using System;
using System.Threading.Tasks;
using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DigitalSignatureController : ControllerBase
    {
        private readonly IDigitalSignatureService _service;

        public DigitalSignatureController(IDigitalSignatureService service)
        {
            _service = service;
        }

        // ── 1. POST /api/digitalsignature/request ─────────────────────────────
        /// <summary>Manager requests an employee to sign a document.</summary>
        [HttpPost("request")]
        [Consumes("multipart/form-data")] // Forces Swagger to show the file upload UI
        [HasPermission("DIGITALSIGNATURE_REQUEST")]
        public async Task<IActionResult> RequestSignature([FromForm] DigitalSignatureRequestCreateDto dto)
        {
            var result = await _service.RequestSignatureAsync(dto);
            return Ok(result);
        }

        // ... rest of your SignDocument, GetStatus, and GetHistory methods remain the same


        // ── 2. POST /api/digitalsignature/sign/{id} ───────────────────────────
        /// <summary>Employee signs the document.</summary>
        [HttpPost("sign/{id}")]
        [HasPermission("DIGITALSIGNATURE_CREATE")]
        public async Task<IActionResult> SignDocument(int id, [FromForm] DigitalSignatureSignCreateDto dto)
        {
            try
            {
                var result = await _service.SignDocumentAsync(id, dto);
                return Ok(new { message = "Document signed successfully.", data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── 3. GET /api/digitalsignature/status/{id} ──────────────────────────
        /// <summary>Check the status of a signature request.</summary>
        [HttpGet("status/{id}")]
        [HasPermission("DIGITALSIGNATURE_VIEW")]
        public async Task<IActionResult> GetStatus(int id)
        {
            var result = await _service.GetStatusAsync(id);
            if (result is null)
                return NotFound(new { message = $"Signature request {id} not found." });

            return Ok(result);
        }

        // ── 4. GET /api/digitalsignature/history/{employeeId} ─────────────────
        /// <summary>Get all signature history for an employee.</summary>
        [HttpGet("history/{userId}")]
            [HasPermission("DIGITALSIGNATURE_VIEW")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            var result = await _service.GetHistoryAsync(userId);
            return Ok(result);
        }
        // ── 5. PUT /api/digitalsignature/update/{id} ─────────────────────────
        [HttpPut("update/{id}")]
        [HasPermission("DIGITALSIGNATURE_UPDATE")]
        public async Task<IActionResult> UpdateRequest(int id, [FromForm] DigitalSignatureRequestCreateDto dto)
        {
            try
            {
                var result = await _service.UpdateRequestAsync(id, dto);
                return Ok(new { message = "Signature request updated successfully.", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        // ── 7. GET /api/digitalsignature/all ─────────────────────────
        [HttpGet("all")]
        [HasPermission("DIGITALSIGNATURE_VIEW")]
        public async Task<IActionResult> GetAllSignatures()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        // ── 6. DELETE /api/digitalsignature/delete/{id} ───────────────────────
        [HttpDelete("delete/{id}")]
        [HasPermission("DIGITALSIGNATURE_DELETE")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            try
            {
                await _service.DeleteRequestAsync(id);
                return Ok(new { message = "Signature request deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
    
}


