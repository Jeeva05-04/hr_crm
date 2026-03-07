using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IEmployeeOnboardingService
    {
        Task<EmployeeOnboarding> CreateAsync(EmployeeOnboardingCreateDto dto);

        Task<List<EmployeeOnboarding>> GetAllAsync();

        Task<EmployeeOnboarding?> GetByIdAsync(int id);

        Task<bool> DeleteAsync(int id);
    }
}