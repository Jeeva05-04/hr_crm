using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IJobOpeningRepository
    {
        Task<List<JobOpening>> GetAllAsync();
        Task<List<JobOpening>> GetByDepartmentAsync(int departmentId);
        Task<JobOpening?> GetByIdAsync(int jobOpeningId);
        Task<JobOpening> AddAsync(JobOpening jobOpening);
        Task<JobOpening> UpdateAsync(JobOpening jobOpening);
        Task DeleteAsync(int jobOpeningId);
    }
}
