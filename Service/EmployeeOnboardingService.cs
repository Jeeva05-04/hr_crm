using AutoMapper;
using hr_crm.Data;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Services
{
    public class EmployeeOnboardingService : IEmployeeOnboardingService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EmployeeOnboardingService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Save a single file to the given folder, return relative URL path
        private async Task<string?> SaveFileAsync(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0) return null;

            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(folder, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fullPath;
        }

        // Save multiple files (e.g. ParentAadhar list), returns semicolon-joined paths
        private async Task<string?> SaveFilesAsync(List<IFormFile>? files, string folder)
        {
            if (files == null || files.Count == 0) return null;

            var paths = new List<string>();
            foreach (var file in files)
            {
                var path = await SaveFileAsync(file, folder);
                if (path != null) paths.Add(path);
            }
            return paths.Count > 0 ? string.Join(";", paths) : null;
        }

        public async Task<EmployeeOnboarding> CreateAsync(EmployeeOnboardingCreateDto dto, string webRootPath)
        {
            var onboarding = _mapper.Map<EmployeeOnboarding>(dto);

            onboarding.DateOfJoining = DateTime.SpecifyKind(dto.DateOfJoining, DateTimeKind.Utc);
            onboarding.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc);
            onboarding.FatherDOB = DateTime.SpecifyKind(dto.FatherDOB, DateTimeKind.Utc);
            onboarding.MotherDOB = DateTime.SpecifyKind(dto.MotherDOB, DateTimeKind.Utc);

            if (dto.SpouseDOB.HasValue)
                onboarding.SpouseDOB = DateTime.SpecifyKind(dto.SpouseDOB.Value, DateTimeKind.Utc);

            onboarding.CreatedDate = DateTime.UtcNow;

            _context.EmployeeOnboardings.Add(onboarding);
            await _context.SaveChangesAsync();

            var employeeFolder = Path.Combine(webRootPath, "onboarding", onboarding.EmployeeOnboardingId.ToString());

            // Save Documents
            var documents = new EmployeeOnboardingDocuments
            {
                EmployeeOnboardingId = onboarding.EmployeeOnboardingId,
                AadharCardPath         = await SaveFileAsync(dto.AadharCard, employeeFolder),
                PANCardPath            = await SaveFileAsync(dto.PANCard, employeeFolder),
                BankStatementPath      = await SaveFileAsync(dto.BankStatement, employeeFolder),
                BankPassbookPath       = await SaveFileAsync(dto.BankPassbook, employeeFolder),
                ParentAadharPath       = await SaveFilesAsync(dto.ParentAadhar, employeeFolder),
                HighestQualificationDocumentPath = await SaveFileAsync(dto.HighestQualificationDocument, employeeFolder),
                ExperienceLetterPath   = await SaveFileAsync(dto.ExperienceLetter, employeeFolder),
                AcceptanceLetterPath   = await SaveFileAsync(dto.AcceptanceLetter, employeeFolder),
            };
            _context.EmployeeOnboardingDocuments.Add(documents);

            // Save Work Experience
            var workExp = _mapper.Map<WorkExperience>(dto);
            workExp.EmployeeOnboardingId = onboarding.EmployeeOnboardingId;
            workExp.PreviousCompanyPayslipPath = await SaveFileAsync(dto.PreviousCompanyPayslip, employeeFolder);
            _context.WorkExperiences.Add(workExp);

            // Save Laptop Image (stored on main entity)
            if (dto.LaptopImage != null)
                onboarding.LaptopImagePath = await SaveFileAsync(dto.LaptopImage, employeeFolder);

            await _context.SaveChangesAsync();

            return onboarding;
        }

        public async Task<List<EmployeeOnboarding>> GetAllAsync()
        {
            return await _context.EmployeeOnboardings.ToListAsync();
        }

        public async Task<EmployeeOnboarding?> GetByIdAsync(int id)
        {
            return await _context.EmployeeOnboardings
                .FirstOrDefaultAsync(x => x.EmployeeOnboardingId == id);
        }

        public async Task<EmployeeOnboardingDocuments?> GetDocumentsAsync(int onboardingId)
        {
            return await _context.EmployeeOnboardingDocuments
                .FirstOrDefaultAsync(d => d.EmployeeOnboardingId == onboardingId);
        }

        public async Task<WorkExperience?> GetWorkExperienceAsync(int onboardingId)
        {
            return await _context.WorkExperiences
                .FirstOrDefaultAsync(w => w.EmployeeOnboardingId == onboardingId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var onboarding = await _context.EmployeeOnboardings
                .FirstOrDefaultAsync(x => x.EmployeeOnboardingId == id);

            if (onboarding == null) return false;

            _context.EmployeeOnboardings.Remove(onboarding);
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // INVITE — generate shareable link token
        // =============================================
        public async Task<OnboardingInvite> GenerateInviteAsync(GenerateInviteDto dto, int createdByUserId, string baseUrl)
        {
            var rawToken = Guid.NewGuid().ToString("N");   // 32-char hex, stored in DB

            var invite = new OnboardingInvite
            {
                Token = rawToken,
                EmployeeEmail = dto.EmployeeEmail,
                EmployeeName = dto.EmployeeName,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsUsed = false
            };

            _context.OnboardingInvites.Add(invite);
            await _context.SaveChangesAsync();

            // Return a copy with the full shareable link in the Token field (not saved to DB)
            return new OnboardingInvite
            {
                Id = invite.Id,
                Token = rawToken,
                EmployeeEmail = invite.EmployeeEmail,
                EmployeeName = invite.EmployeeName,
                CreatedByUserId = invite.CreatedByUserId,
                CreatedAt = invite.CreatedAt,
                ExpiresAt = invite.ExpiresAt,
                IsUsed = false,
                OnboardingId = null
            };
        }

        // =============================================
        // VALIDATE — check if token is usable
        // =============================================
        public async Task<(bool Valid, string? Error, OnboardingInvite? Invite)> ValidateTokenAsync(string token)
        {
            var invite = await _context.OnboardingInvites
                .FirstOrDefaultAsync(i => i.Token == token);

            if (invite == null)
                return (false, "Invalid or unknown invite link.", null);

            if (invite.IsUsed)
                return (false, "This invite link has already been used.", null);

            if (DateTime.UtcNow > invite.ExpiresAt)
                return (false, "This invite link has expired. Please request a new one from HR.", null);

            return (true, null, invite);
        }

        // =============================================
        // SUBMIT — employee submits form using token
        // =============================================
        public async Task<(EmployeeOnboarding? Record, string? Error)> SubmitWithTokenAsync(string token, EmployeeOnboardingCreateDto dto, string webRootPath)
        {
            var invite = await _context.OnboardingInvites
                .FirstOrDefaultAsync(i => i.Token == token);

            if (invite == null)
                return (null, "Invalid or unknown invite link.");

            if (invite.IsUsed)
                return (null, "This invite link has already been used.");

            if (DateTime.UtcNow > invite.ExpiresAt)
                return (null, "This invite link has expired. Please request a new one from HR.");

            var record = await CreateAsync(dto, webRootPath);

            // Mark invite as used and link to the submitted record
            invite.IsUsed = true;
            invite.OnboardingId = record.EmployeeOnboardingId;
            await _context.SaveChangesAsync();

            return (record, null);
        }

        // =============================================
        // GET ALL INVITES — for HR manager
        // =============================================
        public async Task<List<OnboardingInvite>> GetAllInvitesAsync()
        {
            return await _context.OnboardingInvites
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
    }
}
