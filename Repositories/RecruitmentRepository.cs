using hr_crm.Data;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class RecruitmentRepository : IRecruitmentRepository
    {
        private readonly AppDbContext _context;

        public RecruitmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Recruitment>> GetAllAsync()
        {
            return await _context.Recruitments
                .OrderByDescending(r => r.ApplicationDate)
                .ToListAsync();
        }

        public async Task<Recruitment?> GetByIdAsync(int id)
        {
            return await _context.Recruitments
                .FirstOrDefaultAsync(r => r.CandidateId == id);
        }

        public async Task AddAsync(Recruitment recruitment)
        {
            _context.Recruitments.Add(recruitment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Recruitment recruitment)
        {
            _context.Recruitments.Update(recruitment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var candidate = await _context.Recruitments
                .FirstOrDefaultAsync(r => r.CandidateId == id);

            if (candidate != null)
            {
                _context.Recruitments.Remove(candidate);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Recruitment>> GetByStatusAsync(string status)
        {
            return await _context.Recruitments
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.ApplicationDate)
                .ToListAsync();
        }

        // Convert selected candidate → create EmployeeOnboarding stub
        public async Task<EmployeeOnboarding> ConvertToOnboardingAsync(Recruitment candidate)
        {
            var onboarding = new EmployeeOnboarding
            {
                FullName = $"{candidate.FirstName} {candidate.LastName}",
                Email = candidate.Email,
                MobileNumber = candidate.Phone,
                DateOfJoining = DateTime.UtcNow,
                // Required non-null fields — will be filled during full onboarding
                DateOfBirth = DateTime.UtcNow,
                BloodGroup = "",
                MaritalStatus = "",
                FatherName = "",
                FatherDOB = DateTime.UtcNow,
                IsFatherDeceased = "No",
                MotherName = "",
                MotherDOB = DateTime.UtcNow,
                IsMotherDeceased = "No",
                PAN = "",
                AadharNumber = "",
                EmergencyContactName = "",
                EmergencyContactRelationship = "",
                TemporaryAddress = "",
                PermanentAddress = "",
                BankName = "",
                AccountNumber = "",
                IFSC = "",
                BranchName = "",
                OfficeEmail = "",
                OfficeMobileNumber = "",
                LaptopSerialNumber = "",
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _context.EmployeeOnboardings.Add(onboarding);

            // Link onboarding back to recruitment
            candidate.OnboardingId = onboarding.EmployeeOnboardingId;
            candidate.Status = "Onboarded";

            await _context.SaveChangesAsync();

            // Now OnboardingId is populated — update candidate
            candidate.OnboardingId = onboarding.EmployeeOnboardingId;
            _context.Recruitments.Update(candidate);
            await _context.SaveChangesAsync();

            return onboarding;
        }

        public async Task<bool> AssignLeadAsync(int candidateId, int assignedToUserId)
        {
            var candidate = await _context.Recruitments
                .FirstOrDefaultAsync(r => r.CandidateId == candidateId);
            if (candidate == null) return false;

            candidate.AssignedToUserId = assignedToUserId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RecruitmentDashboardDto> GetDashboardAsync()
        {
            var recruitments = await _context.Recruitments.ToListAsync();
            var jobOpenings = await _context.JobOpenings.Include(j => j.Department).ToListAsync();
            var departments = await _context.Departments.ToListAsync();

            var statusGroups = recruitments
                .GroupBy(r => r.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var byDepartment = departments.Select(d =>
            {
                var deptCandidates = recruitments.Where(r => r.DepartmentId == d.DepartmentId).ToList();
                return new DepartmentStatsDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    Openings = jobOpenings.Count(j => j.DepartmentId == d.DepartmentId && j.Status == "Open"),
                    Applicants = deptCandidates.Count,
                    InterviewsDone = deptCandidates.Count(r => r.Status == "InterviewScheduled" || r.InterviewDate != null),
                    OnHold = deptCandidates.Count(r => r.Status == "OnHold"),
                    Selected = deptCandidates.Count(r => r.Status == "Selected"),
                    Hired = deptCandidates.Count(r => r.Status == "Onboarded"),
                    Rejected = deptCandidates.Count(r => r.Status == "Rejected")
                };
            })
            .Where(d => d.Applicants > 0 || d.Openings > 0)
            .ToList();

            var byRole = recruitments
                .GroupBy(r => r.AppliedPosition)
                .Select(g => new RoleStatsDto
                {
                    Role = g.Key,
                    Openings = jobOpenings.Count(j => j.Title == g.Key && j.Status == "Open"),
                    Applicants = g.Count(),
                    InterviewsDone = g.Count(r => r.Status == "InterviewScheduled" || r.InterviewDate != null),
                    OnHold = g.Count(r => r.Status == "OnHold"),
                    Selected = g.Count(r => r.Status == "Selected"),
                    Offered = g.Count(r => r.Status == "Offered"),
                    Hired = g.Count(r => r.Status == "Onboarded"),
                    Rejected = g.Count(r => r.Status == "Rejected")
                })
                .ToList();

            return new RecruitmentDashboardDto
            {
                TotalOpenings = jobOpenings.Count(j => j.Status == "Open"),
                TotalApplicants = recruitments.Count,
                ByStatus = new StatusBreakdownDto
                {
                    Applied = statusGroups.GetValueOrDefault("Applied", 0),
                    Screening = statusGroups.GetValueOrDefault("Screening", 0),
                    InterviewScheduled = statusGroups.GetValueOrDefault("InterviewScheduled", 0),
                    OnHold = statusGroups.GetValueOrDefault("OnHold", 0),
                    Selected = statusGroups.GetValueOrDefault("Selected", 0),
                    Offered = statusGroups.GetValueOrDefault("Offered", 0),
                    Hired = statusGroups.GetValueOrDefault("Onboarded", 0),
                    Rejected = statusGroups.GetValueOrDefault("Rejected", 0)
                },
                ByDepartment = byDepartment,
                ByRole = byRole
            };
        }

        public async Task<RoleStatsDto?> GetDashboardByRoleAsync(string role)
        {
            var candidates = await _context.Recruitments
                .Where(r => r.AppliedPosition == role)
                .ToListAsync();

            if (!candidates.Any()) return null;

            var openings = await _context.JobOpenings
                .CountAsync(j => j.Title == role && j.Status == "Open");

            return new RoleStatsDto
            {
                Role = role,
                Openings = openings,
                Applicants = candidates.Count,
                InterviewsDone = candidates.Count(r => r.Status == "InterviewScheduled" || r.InterviewDate != null),
                OnHold = candidates.Count(r => r.Status == "OnHold"),
                Selected = candidates.Count(r => r.Status == "Selected"),
                Offered = candidates.Count(r => r.Status == "Offered"),
                Hired = candidates.Count(r => r.Status == "Onboarded"),
                Rejected = candidates.Count(r => r.Status == "Rejected")
            };
        }

        public async Task<DepartmentStatsDto?> GetDashboardByDepartmentAsync(int departmentId)
        {
            var dept = await _context.Departments.FindAsync(departmentId);
            if (dept == null) return null;

            var candidates = await _context.Recruitments
                .Where(r => r.DepartmentId == departmentId)
                .ToListAsync();

            var openings = await _context.JobOpenings
                .CountAsync(j => j.DepartmentId == departmentId && j.Status == "Open");

            return new DepartmentStatsDto
            {
                DepartmentId = departmentId,
                DepartmentName = dept.DepartmentName,
                Openings = openings,
                Applicants = candidates.Count,
                InterviewsDone = candidates.Count(r => r.Status == "InterviewScheduled" || r.InterviewDate != null),
                OnHold = candidates.Count(r => r.Status == "OnHold"),
                Selected = candidates.Count(r => r.Status == "Selected"),
                Hired = candidates.Count(r => r.Status == "Onboarded"),
                Rejected = candidates.Count(r => r.Status == "Rejected")
            };
        }
    }
}