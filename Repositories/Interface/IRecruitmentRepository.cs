using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IRecruitmentRepository
    {
        Task<List<Recruitment>> GetAllAsync();
        Task<Recruitment?> GetByIdAsync(int id);
        Task AddAsync(Recruitment recruitment);
        Task UpdateAsync(Recruitment recruitment);
        Task DeleteAsync(int id);
        Task<List<Recruitment>> GetByStatusAsync(string status);
        Task<EmployeeOnboarding> ConvertToOnboardingAsync(Recruitment candidate);
        Task<bool> AssignLeadAsync(int candidateId, int assignedToUserId);
    }
}