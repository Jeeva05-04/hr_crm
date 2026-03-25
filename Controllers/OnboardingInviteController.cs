using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnboardingInviteController : ControllerBase
    {
        private readonly IEmployeeOnboardingService _service;
        private readonly IConfiguration _config;

        public OnboardingInviteController(IEmployeeOnboardingService service, IConfiguration config)
        {
            _service = service;
            _config = config;
        }

        // =============================================
        // HR MANAGER: Generate a shareable invite link
        // POST /api/onboardinginvite/generate
        // =============================================
        [HttpPost("generate")]
        [HasPermission("ONBOARDING_INVITE_GENERATE")]
        public async Task<IActionResult> Generate([FromBody] GenerateInviteDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            var createdByUserId = int.Parse(userIdClaim.Value);

            var baseUrl = _config["FrontendBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";

            var invite = await _service.GenerateInviteAsync(dto, createdByUserId, baseUrl);

            var shareableLink = $"{baseUrl}/nafa-barter-onboarding-invite.html?token={invite.Token}";

            return Ok(new
            {
                Message = "Invite link generated successfully. Share this link with the employee.",
                ShareableLink = shareableLink,
                Token = invite.Token,
                EmployeeName = invite.EmployeeName,
                EmployeeEmail = invite.EmployeeEmail,
                ExpiresAt = invite.ExpiresAt,
                Note = "This link expires in 7 days and can only be used once."
            });
        }

        // =============================================
        // PUBLIC: Validate token before rendering form
        // GET /api/onboardinginvite/validate/{token}
        // =============================================
        [HttpGet("validate/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> Validate(string token)
        {
            var (valid, error, invite) = await _service.ValidateTokenAsync(token);

            if (!valid)
                return BadRequest(new { Message = error });

            return Ok(new
            {
                Valid = true,
                EmployeeName = invite!.EmployeeName,
                EmployeeEmail = invite.EmployeeEmail,
                ExpiresAt = invite.ExpiresAt
            });
        }

        // =============================================
        // PUBLIC: Employee submits onboarding form via invite token
        // POST /api/onboardinginvite/submit/{token}
        // =============================================
        [HttpPost("submit/{token}")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Submit(string token, [FromForm] EmployeeOnboardingCreateDto dto)
        {
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var (record, error) = await _service.SubmitWithTokenAsync(token, dto, webRootPath);

            if (record == null)
                return BadRequest(new { Message = error });

            return Ok(new
            {
                Message = "Onboarding form submitted successfully.",
                OnboardingId = record.EmployeeOnboardingId,
                EmployeeName = record.FullName
            });
        }

        // =============================================
        // HR MANAGER: View all generated invites
        // GET /api/onboardinginvite/all
        // =============================================
        [HttpGet("all")]
        [HasPermission("ONBOARDING_INVITE_VIEW")]
        public async Task<IActionResult> GetAll()
        {
            var invites = await _service.GetAllInvitesAsync();
            return Ok(invites);
        }
    }
}
