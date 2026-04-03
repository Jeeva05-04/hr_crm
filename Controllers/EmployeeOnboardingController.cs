using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hr_crm.Service.Interface;
using hr_crm.DTO;
using System.IO.Compression;
using System.IO;
using System.Linq;

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
        // Return document URLs for an onboarding record (public URLs)
        // GET /api/employeeonboarding/{id}/documents
        // By default this endpoint returns a JSON list of { Label, Url, Exists }.
        // If ?zip=true or Accept: application/zip is sent, it returns a ZIP of the files.
        // =============================================
        // NOTE: helper EnsurePublicFile copies files into wwwroot/onboarding/{id} when possible
        string? EnsurePublicFile(string? storedPath, int onboardingId)
        {
            if (string.IsNullOrEmpty(storedPath)) return null;
            var fileName = Path.GetFileName(storedPath);
            if (string.IsNullOrEmpty(fileName)) return null;

            var destFolder = Path.Combine(_env.WebRootPath, "onboarding", onboardingId.ToString());
            Directory.CreateDirectory(destFolder);
            var destPath = Path.Combine(destFolder, fileName);

            // If already present in onboarding folder, return it
            if (System.IO.File.Exists(destPath)) return destPath;

            // If storedPath exists (absolute or relative), copy to onboarding folder
            if (System.IO.File.Exists(storedPath))
            {
                try { System.IO.File.Copy(storedPath, destPath, overwrite: true); return destPath; } catch { }
            }

            // Check common locations under webroot
            var candidate = Path.Combine(_env.WebRootPath, "onboarding", onboardingId.ToString(), fileName);
            if (System.IO.File.Exists(candidate)) return candidate;
            candidate = Path.Combine(_env.WebRootPath, fileName);
            if (System.IO.File.Exists(candidate))
            {
                try { System.IO.File.Copy(candidate, destPath, overwrite: true); return destPath; } catch { }
            }
            candidate = Path.Combine(_env.WebRootPath, "uploads", fileName);
            if (System.IO.File.Exists(candidate))
            {
                try { System.IO.File.Copy(candidate, destPath, overwrite: true); return destPath; } catch { }
            }

            // Last resort: search webroot for the filename and copy
            try
            {
                var found = Directory.EnumerateFiles(_env.WebRootPath, fileName, SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrEmpty(found) && System.IO.File.Exists(found))
                {
                    try { System.IO.File.Copy(found, destPath, overwrite: true); return destPath; } catch { }
                }
            }
            catch { }

            return null;
        }


        // NOTE: proxy endpoint removed. All documents are served via public URLs under
        // /onboarding/{id}/{filename}. The controller ensures files are copied into that
        // folder when possible so HR can download directly.

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
            var list = await _service.GetAllAsync();
            // Convert any stored filesystem paths to public URLs
            foreach (var dto in list)
            {
                TryConvertPathsToPublicUrls(dto);
            }
            return Ok(list);
        }

        // =============================================
        // GET BY ID
        // =============================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            TryConvertPathsToPublicUrls(result);
            return Ok(result);
        }

        // Convert DTO file-system paths to public URLs under /onboarding/{id}/{filename}
        private void TryConvertPathsToPublicUrls(DTO.EmployeeOnboardingResponseDto dto)
        {
            if (dto == null) return;
            int id = dto.EmployeeOnboardingId;
            string MakePublicUrl(string fileName) => $"{Request.Scheme}://{Request.Host}/onboarding/{id}/{Uri.EscapeDataString(fileName)}";

            string? Convert(string? storedPath)
            {
                if (string.IsNullOrEmpty(storedPath)) return null;
                var publicPath = EnsurePublicFile(storedPath, id);
                if (string.IsNullOrEmpty(publicPath)) return null;
                var fileName = Path.GetFileName(publicPath);
                return MakePublicUrl(fileName);
            }

            try
            {
                dto.PreviousCompanyPayslipPath = Convert(dto.PreviousCompanyPayslipPath);
                dto.AadharCardPath = Convert(dto.AadharCardPath);
                dto.PANCardPath = Convert(dto.PANCardPath);
                dto.BankStatementPath = Convert(dto.BankStatementPath);
                dto.BankPassbookPath = Convert(dto.BankPassbookPath);
                dto.ParentAadharPath = Convert(dto.ParentAadharPath);
                dto.HighestQualificationDocumentPath = Convert(dto.HighestQualificationDocumentPath);
                dto.ExperienceLetterPath = Convert(dto.ExperienceLetterPath);
                dto.AcceptanceLetterPath = Convert(dto.AcceptanceLetterPath);
                dto.LaptopImagePath = Convert(dto.LaptopImagePath);
            }
            catch { /* silently ignore conversion errors */ }
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
            // Collect and resolve files (same logic used by document-urls)
            var files = new List<(string Label, string Path)>();

            string? ResolvePath(string? storedPath)
            {
                if (string.IsNullOrEmpty(storedPath)) return null;
                if (System.IO.File.Exists(storedPath)) return storedPath;
                try
                {
                    var fileName = Path.GetFileName(storedPath);
                    if (string.IsNullOrEmpty(fileName)) return null;
                    var candidate = Path.Combine(_env.WebRootPath, "onboarding", id.ToString(), fileName);
                    if (System.IO.File.Exists(candidate)) return candidate;
                    candidate = Path.Combine(_env.WebRootPath, fileName);
                    if (System.IO.File.Exists(candidate)) return candidate;
                    var trimmed = storedPath.TrimStart('~', '/', '\\');
                    candidate = Path.Combine(_env.WebRootPath, trimmed.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(candidate)) return candidate;
                }
                catch { }
                return null;
            }

            void Add(string label, string? path)
            {
                var resolved = ResolvePath(path);
                if (!string.IsNullOrEmpty(resolved)) files.Add((label, resolved));
            }

            Add("Aadhar_Card", docs?.AadharCardPath);
            Add("PAN_Card", docs?.PANCardPath);
            Add("Bank_Statement", docs?.BankStatementPath);
            Add("Bank_Passbook", docs?.BankPassbookPath);
            Add("Highest_Qualification", docs?.HighestQualificationDocumentPath);
            Add("Experience_Letter", docs?.ExperienceLetterPath);
            Add("Acceptance_Letter", docs?.AcceptanceLetterPath);
            Add("Previous_Company_Payslip", work?.PreviousCompanyPayslipPath);
            Add("Laptop_Image", onboarding.LaptopImagePath);

            if (!string.IsNullOrEmpty(docs?.ParentAadharPath))
            {
                var parts = docs.ParentAadharPath.Split(';');
                for (int i = 0; i < parts.Length; i++)
                {
                    var resolved = ResolvePath(parts[i]);
                    if (!string.IsNullOrEmpty(resolved)) files.Add(($"Parent_Aadhar_{i + 1}", resolved));
                }
            }

            // Fallback to enumerate onboarding folder
            if (files.Count == 0)
            {
                try
                {
                    var folder = Path.Combine(_env.WebRootPath, "onboarding", id.ToString());
                    if (Directory.Exists(folder))
                    {
                        var physicalFiles = Directory.GetFiles(folder);
                        foreach (var f in physicalFiles)
                        {
                            if (Path.GetFileName(f).StartsWith('.')) continue;
                            files.Add((Path.GetFileNameWithoutExtension(f), f));
                        }
                    }
                }
                catch { }

            // Additionally include any recently uploaded files under wwwroot/uploads
            // (this helps capture images/signatures uploaded separately but not referenced in the onboarding DB)
            try
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (Directory.Exists(uploadsFolder))
                {
                    // Consider files modified between onboarding.CreatedDate -1 day and +7 days
                    var windowStart = onboarding.CreatedDate.ToUniversalTime().AddDays(-1);
                    var windowEnd = onboarding.CreatedDate.ToUniversalTime().AddDays(7);

                    var candidateFiles = Directory.EnumerateFiles(uploadsFolder, "*", SearchOption.AllDirectories);
                    foreach (var f in candidateFiles)
                    {
                        try
                        {
                            var wt = System.IO.File.GetLastWriteTimeUtc(f);
                            if (wt >= windowStart && wt <= windowEnd)
                            {
                                // Copy into onboarding public folder and add
                                var publicPath = EnsurePublicFile(f, id);
                                if (!string.IsNullOrEmpty(publicPath) && !files.Any(x => string.Equals(x.Path, publicPath, StringComparison.OrdinalIgnoreCase)))
                                {
                                    files.Add((Path.GetFileNameWithoutExtension(f), publicPath));
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
            }

            // If client requested ZIP (query ?zip=true) then return ZIP; otherwise return document URLs list
            var zipRequested = string.Equals(Request.Query["zip"].FirstOrDefault(), "true", StringComparison.OrdinalIgnoreCase)
                               || Request.Headers["Accept"].Any(h => h != null && h.Contains("application/zip"));

            if (files.Count == 0)
                return NotFound(new { Message = "No documents uploaded for this employee." });

            if (zipRequested)
            {
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

            // Build URL list for frontend (public URLs under /onboarding/{id}/{filename})
            string MakePublicUrl(string fileName) => $"{Request.Scheme}://{Request.Host}/onboarding/{id}/{Uri.EscapeDataString(fileName)}";

            var urls = new List<object>();
            foreach (var (label, path) in files)
            {
                var fileName = Path.GetFileName(path);
                var webPath = Path.Combine(_env.WebRootPath, "onboarding", id.ToString(), fileName);
                if (System.IO.File.Exists(webPath)) urls.Add(new { Label = label, Url = MakePublicUrl(fileName), Exists = true });
                else urls.Add(new { Label = label, Url = (string?)null, Exists = false });
            }

            return Ok(urls);
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
