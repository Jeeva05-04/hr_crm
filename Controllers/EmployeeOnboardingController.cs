using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hr_crm.Service.Interface;
using hr_crm.DTO;
using System.IO.Compression;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeOnboardingController : ControllerBase
    {
        // =============================================
        // PUBLIC SUBMIT — Employee fills form via invite link
        // POST /api/employeeonboarding/submit/{token}
        // =============================================
        private readonly IEmployeeOnboardingService _service;
        private readonly IWebHostEnvironment _env;

        public EmployeeOnboardingController(IEmployeeOnboardingService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // =============================================
        // PUBLIC SUBMIT — No token, open for all employees
        // POST /api/employeeonboarding/public-submit
        // =============================================
        [HttpPost("public-submit")]
        [AllowAnonymous]
        public async Task<IActionResult> PublicSubmit([FromForm] EmployeeOnboardingCreateDto dto)
        {
            var record = await _service.CreateAsync(dto, _env.WebRootPath);

            return Ok(new
            {
                Message = "Onboarding form submitted successfully. HR will review your details.",
                record.EmployeeOnboardingId,
                record.FullName,
                record.Email,
                record.CreatedDate
            });
        }

        // =============================================
        // CREATE — saves all documents to disk
        // =============================================
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] EmployeeOnboardingCreateDto dto)
        {
            var result = await _service.CreateAsync(dto, _env.WebRootPath);
            return Ok(result);
        }

        // =============================================
        // GET ALL
        // =============================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // =============================================
        // GET BY ID
        // =============================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // =============================================
        // DOWNLOAD ALL DOCUMENTS AS ZIP
        // GET /api/employeeonboarding/{id}/documents
        // =============================================
        [HttpGet("{id}/documents")]
        public async Task<IActionResult> DownloadAllDocuments(int id)
        {
            var onboarding = await _service.GetByIdAsync(id);
            if (onboarding == null)
                return NotFound(new { Message = "Onboarding record not found." });

            var docs = await _service.GetDocumentsAsync(id);
            var work = await _service.GetWorkExperienceAsync(id);

            // Collect all uploaded files: (label, filePath)
            var files = new List<(string Label, string Path)>();

            void Add(string label, string? path)
            {
                if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                    files.Add((label, path));
            }

            Add("Aadhar_Card",                   docs?.AadharCardPath);
            Add("PAN_Card",                      docs?.PANCardPath);
            Add("Bank_Statement",                docs?.BankStatementPath);
            Add("Bank_Passbook",                 docs?.BankPassbookPath);
            Add("Highest_Qualification",         docs?.HighestQualificationDocumentPath);
            Add("Experience_Letter",             docs?.ExperienceLetterPath);
            Add("Acceptance_Letter",             docs?.AcceptanceLetterPath);
            Add("Previous_Company_Payslip",      work?.PreviousCompanyPayslipPath);
            Add("Laptop_Image",                  onboarding.LaptopImagePath);

            // Parent Aadhar may have multiple files (semicolon-separated)
            if (!string.IsNullOrEmpty(docs?.ParentAadharPath))
            {
                var parts = docs.ParentAadharPath.Split(';');
                for (int i = 0; i < parts.Length; i++)
                    Add($"Parent_Aadhar_{i + 1}", parts[i]);
            }

            if (files.Count == 0)
                return NotFound(new { Message = "No documents uploaded for this employee." });

            // Build ZIP in memory
            using var memoryStream = new MemoryStream();
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var (label, path) in files)
                {
                    var extension = Path.GetExtension(path);
                    var entry = zip.CreateEntry($"{label}{extension}", CompressionLevel.Fastest);
                    using var entryStream = entry.Open();
                    using var fileStream = System.IO.File.OpenRead(path);
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            memoryStream.Position = 0;
            var safeName = string.Concat(onboarding.FullName.Split(Path.GetInvalidFileNameChars()));
            return File(memoryStream.ToArray(), "application/zip", $"{safeName}_Documents.zip");
        }

        // =============================================
        // DELETE
        // =============================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound("Onboarding record not found");
            return Ok("Employee onboarding deleted successfully");
        }
    }
}
