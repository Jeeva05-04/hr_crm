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

            if (dto.DateOfJoining.HasValue)
                onboarding.DateOfJoining = DateTime.SpecifyKind(dto.DateOfJoining.Value, DateTimeKind.Utc);

            if (dto.DateOfBirth.HasValue)
                onboarding.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc);

            if (dto.FatherDOB.HasValue)
                onboarding.FatherDOB = DateTime.SpecifyKind(dto.FatherDOB.Value, DateTimeKind.Utc);

            if (dto.MotherDOB.HasValue)
                onboarding.MotherDOB = DateTime.SpecifyKind(dto.MotherDOB.Value, DateTimeKind.Utc);

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

        public async Task<List<DTO.EmployeeOnboardingResponseDto>> GetAllAsync()
        {
            var list = await _context.EmployeeOnboardings
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            var result = new List<DTO.EmployeeOnboardingResponseDto>();
            foreach (var onboarding in list)
            {
                var docs = await GetDocumentsAsync(onboarding.EmployeeOnboardingId);
                var work = await GetWorkExperienceAsync(onboarding.EmployeeOnboardingId);

                var resp = new DTO.EmployeeOnboardingResponseDto
                {
                    EmployeeOnboardingId = onboarding.EmployeeOnboardingId,
                    FullName = onboarding.FullName,
                    DateOfJoining = onboarding.DateOfJoining,
                    DateOfBirth = onboarding.DateOfBirth,
                    Email = onboarding.Email,
                    MobileNumber = onboarding.MobileNumber,
                    BloodGroup = onboarding.BloodGroup,
                    MaritalStatus = onboarding.MaritalStatus,
                    SpouseName = onboarding.SpouseName,
                    SpouseDOB = onboarding.SpouseDOB,
                    ChildrenDetails = onboarding.ChildrenDetails,
                    FatherName = onboarding.FatherName,
                    FatherDOB = onboarding.FatherDOB,
                    IsFatherDeceased = onboarding.IsFatherDeceased == "true" || onboarding.IsFatherDeceased == "True",
                    FatherDOD = onboarding.FatherDOD,
                    FatherAge = onboarding.FatherAge,
                    MotherName = onboarding.MotherName,
                    MotherDOB = onboarding.MotherDOB,
                    IsMotherDeceased = onboarding.IsMotherDeceased == "true" || onboarding.IsMotherDeceased == "True",
                    MotherDOD = onboarding.MotherDOD,
                    MotherAge = onboarding.MotherAge,
                    PAN = onboarding.PAN,
                    AadharNumber = onboarding.AadharNumber,
                    EmergencyContactName = onboarding.EmergencyContactName,
                    EmergencyContactRelationship = onboarding.EmergencyContactRelationship,
                    TemporaryAddress = onboarding.TemporaryAddress,
                    PermanentAddress = onboarding.PermanentAddress,
                    BankName = onboarding.BankName,
                    AccountNumber = onboarding.AccountNumber,
                    IFSC = onboarding.IFSC,
                    BranchName = onboarding.BranchName,
                    OfficeEmail = onboarding.OfficeEmail,
                    OfficeMobileNumber = onboarding.OfficeMobileNumber,
                    LaptopSerialNumber = onboarding.LaptopSerialNumber,
                    LaptopImagePath = onboarding.LaptopImagePath,
                    Status = onboarding.Status,
                    ConvertedUserId = onboarding.ConvertedEmployeeId,
                    ConvertedAt = onboarding.ConvertedAt,
                    IsConvertedToUser = onboarding.ConvertedEmployeeId.HasValue,
                    CreatedDate = onboarding.CreatedDate
                };

                if (docs != null)
                {
                    resp.AadharCardPath = docs.AadharCardPath;
                    resp.PANCardPath = docs.PANCardPath;
                    resp.BankStatementPath = docs.BankStatementPath;
                    resp.BankPassbookPath = docs.BankPassbookPath;
                    resp.ParentAadharPath = docs.ParentAadharPath;
                    resp.HighestQualificationDocumentPath = docs.HighestQualificationDocumentPath;
                    resp.ExperienceLetterPath = docs.ExperienceLetterPath;
                    resp.AcceptanceLetterPath = docs.AcceptanceLetterPath;
                }

                if (work != null)
                {
                    resp.PreviousCompanyDetails = work.PreviousCompanyDetails;
                    resp.OfferedDesignation = work.OfferedDesignation;
                    resp.OfferedSalaryNTH = work.OfferedSalaryNTH;
                    resp.OfferedMonthlyCTC = work.OfferedMonthlyCTC;
                    resp.OfferedYearlyCTC = work.OfferedYearlyCTC;
                    resp.TotalExperience = work.TotalExperience;
                    resp.LastCompanyPFNumber = work.LastCompanyPFNumber;
                    resp.LastCompanyUAN = work.LastCompanyUAN;
                    resp.PreviousCompanyPayslipPath = work.PreviousCompanyPayslipPath;
                }

                result.Add(resp);
            }

            return result;
        }

        public async Task<DTO.EmployeeOnboardingResponseDto?> GetByIdAsync(int id)
        {
            var onboarding = await _context.EmployeeOnboardings
                .FirstOrDefaultAsync(x => x.EmployeeOnboardingId == id);

            if (onboarding == null) return null;

            var docs = await GetDocumentsAsync(id);
            var work = await GetWorkExperienceAsync(id);

            var resp = new DTO.EmployeeOnboardingResponseDto
            {
                EmployeeOnboardingId = onboarding.EmployeeOnboardingId,
                FullName = onboarding.FullName,
                DateOfJoining = onboarding.DateOfJoining,
                DateOfBirth = onboarding.DateOfBirth,
                Email = onboarding.Email,
                MobileNumber = onboarding.MobileNumber,
                BloodGroup = onboarding.BloodGroup,
                MaritalStatus = onboarding.MaritalStatus,
                SpouseName = onboarding.SpouseName,
                SpouseDOB = onboarding.SpouseDOB,
                ChildrenDetails = onboarding.ChildrenDetails,
                FatherName = onboarding.FatherName,
                FatherDOB = onboarding.FatherDOB,
                IsFatherDeceased = onboarding.IsFatherDeceased == "true" || onboarding.IsFatherDeceased == "True",
                FatherDOD = onboarding.FatherDOD,
                FatherAge = onboarding.FatherAge,
                MotherName = onboarding.MotherName,
                MotherDOB = onboarding.MotherDOB,
                IsMotherDeceased = onboarding.IsMotherDeceased == "true" || onboarding.IsMotherDeceased == "True",
                MotherDOD = onboarding.MotherDOD,
                MotherAge = onboarding.MotherAge,
                PAN = onboarding.PAN,
                AadharNumber = onboarding.AadharNumber,
                EmergencyContactName = onboarding.EmergencyContactName,
                EmergencyContactRelationship = onboarding.EmergencyContactRelationship,
                TemporaryAddress = onboarding.TemporaryAddress,
                PermanentAddress = onboarding.PermanentAddress,
                BankName = onboarding.BankName,
                AccountNumber = onboarding.AccountNumber,
                IFSC = onboarding.IFSC,
                BranchName = onboarding.BranchName,
                OfficeEmail = onboarding.OfficeEmail,
                OfficeMobileNumber = onboarding.OfficeMobileNumber,
                LaptopSerialNumber = onboarding.LaptopSerialNumber,
                LaptopImagePath = onboarding.LaptopImagePath,
                Status = onboarding.Status,
                ConvertedUserId = onboarding.ConvertedEmployeeId,
                ConvertedAt = onboarding.ConvertedAt,
                IsConvertedToUser = onboarding.ConvertedEmployeeId.HasValue,
                CreatedDate = onboarding.CreatedDate
            };

            if (docs != null)
            {
                resp.AadharCardPath = docs.AadharCardPath;
                resp.PANCardPath = docs.PANCardPath;
                resp.BankStatementPath = docs.BankStatementPath;
                resp.BankPassbookPath = docs.BankPassbookPath;
                resp.ParentAadharPath = docs.ParentAadharPath;
                resp.HighestQualificationDocumentPath = docs.HighestQualificationDocumentPath;
                resp.ExperienceLetterPath = docs.ExperienceLetterPath;
                resp.AcceptanceLetterPath = docs.AcceptanceLetterPath;
            }

            if (work != null)
            {
                resp.PreviousCompanyDetails = work.PreviousCompanyDetails;
                resp.OfferedDesignation = work.OfferedDesignation;
                resp.OfferedSalaryNTH = work.OfferedSalaryNTH;
                resp.OfferedMonthlyCTC = work.OfferedMonthlyCTC;
                resp.OfferedYearlyCTC = work.OfferedYearlyCTC;
                resp.TotalExperience = work.TotalExperience;
                resp.LastCompanyPFNumber = work.LastCompanyPFNumber;
                resp.LastCompanyUAN = work.LastCompanyUAN;
                resp.PreviousCompanyPayslipPath = work.PreviousCompanyPayslipPath;
            }

            return resp;
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

        public async Task<(AuthUser? User, string? Error)> ConvertToUserAsync(int onboardingId, int actingUserId)
        {
            var onboarding = await _context.EmployeeOnboardings
                .FirstOrDefaultAsync(x => x.EmployeeOnboardingId == onboardingId);

            if (onboarding == null)
                return (null, "Onboarding record not found.");

            if (onboarding.ConvertedEmployeeId.HasValue)
            {
                var existingUser = await _context.AuthUsers
                    .FirstOrDefaultAsync(x => x.UserId == onboarding.ConvertedEmployeeId.Value && x.DeletedAt == null);

                return existingUser == null
                    ? (null, "Onboarding is already marked as converted, but the user record was not found.")
                    : (existingUser, null);
            }

            var work = await GetWorkExperienceAsync(onboardingId);
            var convertedAt = DateTime.UtcNow;

            // Keep the Postgres identity sequence aligned with the actual max user_id.
            // This avoids stale sequence values and lets the database assign the next id safely.
            await _context.Database.ExecuteSqlRawAsync(
                """
                SELECT setval(
                    pg_get_serial_sequence('users', 'user_id'),
                    COALESCE((SELECT MAX(user_id) FROM users), 1),
                    EXISTS (SELECT 1 FROM users)
                );
                """);

            var user = new AuthUser
            {
                EmployeeId = onboarding.EmployeeOnboardingId.ToString(),
                UserName = string.IsNullOrWhiteSpace(onboarding.FullName) ? onboarding.Email : onboarding.FullName,
                Emails = string.IsNullOrWhiteSpace(onboarding.OfficeEmail) ? onboarding.Email : onboarding.OfficeEmail,
                Department = null,
                Designation = work?.OfferedDesignation,
                ManagerId = null,
                AssignedTeamId = null,
                AssignedRegion = null,
                AssignedBranch = onboarding.BranchName,
                AccountStatus = "Active",
                LockReason = null,
                AccessStartDate = onboarding.DateOfJoining ?? convertedAt,
                AccessEndDate = null,
                LastActivityAt = null,
                LastAssignedLeadAt = null,
                LastClosedTicketAt = null,
                EmploymentType = null,
                WorkShift = null,
                TermsAcceptedAt = convertedAt,
                Remarks = BuildOnboardingRemarks(onboarding, work),
                CreatedBy = actingUserId,
                UpdatedBy = actingUserId,
                ApprovedBy = actingUserId,
                SecurityReviewedBy = null,
                CreatedAt = convertedAt,
                UpdatedAt = convertedAt,
                DeletedAt = null,
                CreatedVia = "OnboardingConversion",
                DomainId = null,
                Gender = null,
                PayrollAmount = work?.OfferedMonthlyCTC
            };

            _context.AuthUsers.Add(user);
            await _context.SaveChangesAsync();

            onboarding.ConvertedEmployeeId = user.UserId;
            onboarding.ConvertedAt = convertedAt;
            onboarding.Status = "Converted";
            await _context.SaveChangesAsync();

            return (user, null);
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

        private static string BuildOnboardingRemarks(EmployeeOnboarding onboarding, WorkExperience? work)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(onboarding.MobileNumber))
                parts.Add($"Mobile: {onboarding.MobileNumber}");
            if (!string.IsNullOrWhiteSpace(onboarding.BloodGroup))
                parts.Add($"BloodGroup: {onboarding.BloodGroup}");
            if (!string.IsNullOrWhiteSpace(onboarding.PermanentAddress))
                parts.Add($"Address: {onboarding.PermanentAddress}");
            if (!string.IsNullOrWhiteSpace(work?.TotalExperience))
                parts.Add($"Experience: {work.TotalExperience}");
            if (!string.IsNullOrWhiteSpace(onboarding.OfficeMobileNumber))
                parts.Add($"OfficeMobile: {onboarding.OfficeMobileNumber}");

            return string.Join(" | ", parts);
        }
    }
}
