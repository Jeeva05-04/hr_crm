using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using hr_crm.Data;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Authorization;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeOnboardingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public EmployeeOnboardingController(
            AppDbContext context,
            IWebHostEnvironment env,
            IMapper mapper)
        {
            _context = context;
            _env = env;
            _mapper = mapper;
        }

        // ======================================================
        // ✅ CREATE ONBOARDING (MULTIPART FORM)
        // ======================================================
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] EmployeeOnboardingDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                string uploadPath = Path.Combine(_env.WebRootPath, "uploads", "onboarding");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string SaveFile(IFormFile file)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    file.CopyTo(stream);

                    return "/uploads/onboarding/" + fileName;
                }

                // ===== MAIN TABLE =====
                var onboarding = _mapper.Map<EmployeeOnboarding>(dto);
                _context.EmployeeOnboardings.Add(onboarding);
                await _context.SaveChangesAsync();

                // ===== WORK EXPERIENCE =====
                var work = _mapper.Map<EmployeeOnboardingWorkExperience>(dto);
                work.EmployeeOnboardingId = onboarding.EmployeeOnboardingId;

                if (dto.PreviousCompanyPayslip != null)
                    work.PreviousCompanyPayslipPath = SaveFile(dto.PreviousCompanyPayslip);

                _context.EmployeeOnboardingWorkExperiences.Add(work);

                // ===== DOCUMENTS =====
                var documents = _mapper.Map<EmployeeOnboardingDocuments>(dto);
                documents.EmployeeOnboardingId = onboarding.EmployeeOnboardingId;

                if (dto.AadharCard != null)
                    documents.AadharCardPath = SaveFile(dto.AadharCard);

                if (dto.PANCard != null)
                    documents.PANCardPath = SaveFile(dto.PANCard);

                if (dto.BankStatement != null)
                    documents.BankStatementPath = SaveFile(dto.BankStatement);

                if (dto.BankPassbook != null)
                    documents.BankPassbookPath = SaveFile(dto.BankPassbook);

                // MULTIPLE PARENT AADHAR FILES
                if (dto.ParentAadhar != null && dto.ParentAadhar.Count > 0)
                {
                    var paths = new List<string>();

                    foreach (var file in dto.ParentAadhar)
                    {
                        paths.Add(SaveFile(file));
                    }

                    documents.ParentAadharPaths = string.Join(",", paths);
                }

                if (dto.HighestQualificationDocument != null)
                    documents.HighestQualificationDocumentPath = SaveFile(dto.HighestQualificationDocument);

                if (dto.ExperienceLetter != null)
                    documents.ExperienceLetterPath = SaveFile(dto.ExperienceLetter);

                if (dto.AcceptanceLetter != null)
                    documents.AcceptanceLetterPath = SaveFile(dto.AcceptanceLetter);

                _context.EmployeeOnboardingDocuments.Add(documents);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Onboarding submitted successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        // ======================================================
        // ✅ VIEW ALL (HR ONLY)
        // ======================================================
        [HasPermission("ONBOARDING_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.EmployeeOnboardings
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new
                {
                    x.EmployeeOnboardingId,
                    x.FullName,
                    x.Email,
                    x.Status,
                    x.CreatedDate
                })
                .ToListAsync();

            return Ok(list);
        }

        // ======================================================
        // ✅ VIEW FULL DETAILS (INCLUDING WORK + DOCS)
        // ======================================================
        [HasPermission("ONBOARDING_VIEW")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var onboarding = await _context.EmployeeOnboardings
                .FirstOrDefaultAsync(x => x.EmployeeOnboardingId == id);

            if (onboarding == null)
                return NotFound();

            var work = await _context.EmployeeOnboardingWorkExperiences
                .FirstOrDefaultAsync(x => x.EmployeeOnboardingId == id);

            var documents = await _context.EmployeeOnboardingDocuments
                .FirstOrDefaultAsync(x => x.EmployeeOnboardingId == id);

            return Ok(new
            {
                PersonalDetails = onboarding,
                WorkExperience = work,
                Documents = documents
            });
        }
    }
}