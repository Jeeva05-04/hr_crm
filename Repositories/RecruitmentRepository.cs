using hr_crm.Data;
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
    }
}