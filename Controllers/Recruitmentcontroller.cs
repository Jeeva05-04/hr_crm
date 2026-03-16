using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Models;
using hr_crm.Service;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecruitmentController : ControllerBase
    {
        private readonly IRecruitmentService _service;
        private readonly IWebHostEnvironment _env;
        private readonly NotificationService _notification;

        public RecruitmentController(IRecruitmentService service, IWebHostEnvironment env,
            NotificationService notification)
        {
            _service = service;
            _env = env;
            _notification = notification;
        }

        private static RecruitmentResponseDto MapToResponse(Recruitment c) => new()
        {
            CandidateId = c.CandidateId,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            AppliedPosition = c.AppliedPosition,
            DepartmentId = c.DepartmentId,
            ApplicationDate = c.ApplicationDate.ToString("yyyy-MM-dd"),
            Status = c.Status,
            Source = c.Source,
            InterviewDate = c.InterviewDate,
            InterviewerName = c.InterviewerName,
            InterviewType = c.InterviewType,
            InterviewNotes = c.InterviewNotes,
            ExpectedSalary = c.ExpectedSalary,
            OfferedSalary = c.OfferedSalary,
            ResumeUrl = c.ResumeUrl,
            OnboardingId = c.OnboardingId,
            AssignedToUserId = c.AssignedToUserId
        };

        // Save PDF to wwwroot/uploads/resumes/ and return the relative URL path
        private async Task<(string? Path, string? Error)> SaveResumeAsync(IFormFile file, string candidateName)
        {
            if (file.Length == 0)
                return (null, "Resume file is empty.");

            if (file.ContentType != "application/pdf" &&
                !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return (null, "Only PDF files are allowed for resumes.");

            if (file.Length > 5 * 1024 * 1024) // 5 MB limit
                return (null, "Resume file size must not exceed 5 MB.");

            var folder = Path.Combine(_env.WebRootPath, "uploads", "resumes");
            Directory.CreateDirectory(folder);

            // Safe filename: {candidateName}_{timestamp}.pdf
            var safeName = string.Concat(candidateName.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"{safeName}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return ($"/uploads/resumes/{fileName}", null);
        }

        // =========================================
        // GET ALL
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GetCandidates()
        {
            var candidates = await _service.GetAllAsync();
            return Ok(candidates.Select(MapToResponse));
        }

        // =========================================
        // GET BY STATUS — pipeline view
        // =========================================
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var candidates = await _service.GetByStatusAsync(status);
            return Ok(candidates.Select(MapToResponse));
        }

        // =========================================
        // GET BY ID
        // =========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var candidate = await _service.GetByIdAsync(id);
            if (candidate == null)
                return NotFound(new { Message = "Candidate not found." });

            return Ok(MapToResponse(candidate));
        }

        // =========================================
        // CREATE — accepts multipart/form-data (resume PDF optional)
        // =========================================
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddCandidate([FromForm] RecruitmentCreateDto dto)
        {
            string? resumeUrl = null;

            if (dto.Resume != null)
            {
                var (path, error) = await SaveResumeAsync(dto.Resume, $"{dto.FirstName}_{dto.LastName}");
                if (path == null)
                    return BadRequest(new { Message = error });
                resumeUrl = path;
            }

            var recruitment = new Recruitment
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                AppliedPosition = dto.AppliedPosition,
                DepartmentId = dto.DepartmentId,
                ApplicationDate = DateOnly.FromDateTime(dto.ApplicationDate),
                Status = "Applied",
                Source = dto.Source,
                ExpectedSalary = dto.ExpectedSalary,
                ResumeUrl = resumeUrl
            };

            await _service.CreateAsync(recruitment);
            return Ok(new
            {
                Message = "Candidate application added successfully.",
                CandidateId = recruitment.CandidateId,
                ResumeUrl = resumeUrl
            });
        }

        // =========================================
        // UPDATE basic info (no file — use upload-resume for that)
        // =========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCandidate(int id, [FromBody] RecruitmentCreateDto dto)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { Message = "Candidate not found." });

            existing.FirstName = dto.FirstName;
            existing.LastName = dto.LastName;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            existing.AppliedPosition = dto.AppliedPosition;
            existing.DepartmentId = dto.DepartmentId;
            existing.ApplicationDate = DateOnly.FromDateTime(dto.ApplicationDate);
            existing.Source = dto.Source;
            existing.ExpectedSalary = dto.ExpectedSalary;

            await _service.UpdateAsync(existing);
            return Ok(new { Message = "Candidate updated successfully." });
        }

        // =========================================
        // VIEW / DOWNLOAD RESUME
        // =========================================
        [HttpGet("{id}/resume")]
        public async Task<IActionResult> DownloadResume(int id)
        {
            var candidate = await _service.GetByIdAsync(id);
            if (candidate == null)
                return NotFound(new { Message = "Candidate not found." });

            if (string.IsNullOrEmpty(candidate.ResumeUrl))
                return NotFound(new { Message = "No resume uploaded for this candidate." });

            var filePath = Path.Combine(_env.WebRootPath, candidate.ResumeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { Message = "Resume file not found on server." });

            var fileName = $"{candidate.FirstName}_{candidate.LastName}_Resume.pdf";
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }

        // =========================================
        // SCHEDULE INTERVIEW
        // =========================================
        [HttpPut("{id}/schedule-interview")]
        public async Task<IActionResult> ScheduleInterview(int id, [FromBody] ScheduleInterviewDto dto)
        {
            var (success, error) = await _service.ScheduleInterviewAsync(id, dto);
            if (!success)
                return BadRequest(new { Message = error });

            return Ok(new { Message = "Interview scheduled successfully." });
        }

        // =========================================
        // UPDATE STATUS (move through pipeline)
        // =========================================
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var (success, error) = await _service.UpdateStatusAsync(id, dto);
            if (!success)
                return BadRequest(new { Message = error });

            return Ok(new { Message = $"Candidate status updated to '{dto.Status}'." });
        }

        // =========================================
        // CONVERT TO ONBOARDING (Selected → Onboarded)
        // =========================================
        [HttpPost("{id}/convert-to-onboarding")]
        public async Task<IActionResult> ConvertToOnboarding(int id)
        {
            var (onboarding, error) = await _service.ConvertToOnboardingAsync(id);
            if (onboarding == null)
                return BadRequest(new { Message = error });

            return Ok(new
            {
                Message = "Candidate successfully converted to onboarding.",
                OnboardingId = onboarding.EmployeeOnboardingId,
                EmployeeName = onboarding.FullName,
                Note = "Complete the onboarding form using the OnboardingId above."
            });
        }

        // =========================================
        // ASSIGN LEAD TO USER
        // =========================================
        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignLead(int id, [FromBody] AssignLeadDto dto)
        {
            var candidate = await _service.GetByIdAsync(id);
            if (candidate == null)
                return NotFound(new { Message = "Candidate not found." });

            var (success, error) = await _service.AssignLeadAsync(id, dto.AssignedToUserId);
            if (!success)
                return BadRequest(new { Message = error });

            var assignerName = User.FindFirst("name")?.Value
                            ?? User.FindFirst(ClaimTypes.Name)?.Value
                            ?? "HR Manager";

            await _notification.CreateNotification(
                dto.AssignedToUserId,
                $"Lead Assigned: {candidate.FirstName} {candidate.LastName}",
                $"Candidate: {candidate.FirstName} {candidate.LastName}\nPosition: {candidate.AppliedPosition}\nStatus: {candidate.Status}\nAssigned by: {assignerName}",
                "Recruitment",
                id
            );

            return Ok(new { Message = $"Lead assigned to user {dto.AssignedToUserId} successfully." });
        }

        // =========================================
        // DELETE
        // =========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidate(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { Message = "Candidate not found." });

            // Clean up resume file
            if (!string.IsNullOrEmpty(existing.ResumeUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, existing.ResumeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            await _service.DeleteAsync(existing.CandidateId);
            return Ok(new { Message = "Candidate deleted successfully." });
        }
    }
}
