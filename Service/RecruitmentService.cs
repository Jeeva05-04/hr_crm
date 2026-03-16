using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Models;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Services
{
    public class RecruitmentService : IRecruitmentService
    {
        private readonly IRecruitmentRepository _repo;

        public RecruitmentService(IRecruitmentRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Recruitment>> GetAllAsync() => _repo.GetAllAsync();
        public Task<List<Recruitment>> GetByStatusAsync(string status) => _repo.GetByStatusAsync(status);
        public Task<Recruitment?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task CreateAsync(Recruitment recruitment) => _repo.AddAsync(recruitment);
        public Task UpdateAsync(Recruitment recruitment) => _repo.UpdateAsync(recruitment);
        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);

        public async Task<(bool Success, string? Error)> ScheduleInterviewAsync(int candidateId, ScheduleInterviewDto dto)
        {
            var candidate = await _repo.GetByIdAsync(candidateId);
            if (candidate == null)
                return (false, "Candidate not found.");

            if (candidate.Status == "Rejected" || candidate.Status == "Onboarded")
                return (false, $"Cannot schedule interview. Candidate is already '{candidate.Status}'.");

            candidate.InterviewDate = dto.InterviewDate;
            candidate.InterviewerName = dto.InterviewerName;
            candidate.InterviewType = dto.InterviewType;
            candidate.InterviewNotes = dto.Notes;
            candidate.Status = "InterviewScheduled";

            await _repo.UpdateAsync(candidate);
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateStatusAsync(int candidateId, UpdateStatusDto dto)
        {
            var candidate = await _repo.GetByIdAsync(candidateId);
            if (candidate == null)
                return (false, "Candidate not found.");

            var validStatuses = new[] { "Applied", "Screening", "InterviewScheduled", "Selected", "Offered", "Onboarded", "Rejected" };
            if (!validStatuses.Contains(dto.Status))
                return (false, $"Invalid status. Allowed: {string.Join(", ", validStatuses)}");

            if (candidate.Status == "Onboarded")
                return (false, "Candidate is already onboarded. Cannot change status.");

            candidate.Status = dto.Status;

            if (!string.IsNullOrEmpty(dto.Notes))
                candidate.InterviewNotes = dto.Notes;

            if (dto.OfferedSalary.HasValue)
                candidate.OfferedSalary = dto.OfferedSalary;

            await _repo.UpdateAsync(candidate);
            return (true, null);
        }

        public async Task<(EmployeeOnboarding? Onboarding, string? Error)> ConvertToOnboardingAsync(int candidateId)
        {
            var candidate = await _repo.GetByIdAsync(candidateId);
            if (candidate == null)
                return (null, "Candidate not found.");

            if (candidate.Status != "Selected" && candidate.Status != "Offered")
                return (null, $"Only 'Selected' or 'Offered' candidates can be converted. Current status: '{candidate.Status}'.");

            if (candidate.OnboardingId.HasValue)
                return (null, $"Onboarding already exists (ID: {candidate.OnboardingId}). Cannot create duplicate.");

            var onboarding = await _repo.ConvertToOnboardingAsync(candidate);
            return (onboarding, null);
        }

        public async Task<(bool Success, string? Error)> AssignLeadAsync(int candidateId, int assignedToUserId)
        {
            var candidate = await _repo.GetByIdAsync(candidateId);
            if (candidate == null)
                return (false, "Candidate not found.");

            var success = await _repo.AssignLeadAsync(candidateId, assignedToUserId);
            return (success, success ? null : "Failed to assign lead.");
        }
    }
}