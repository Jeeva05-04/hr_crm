using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IRecruitmentService
    {
        Task<List<Recruitment>> GetAllAsync();
        Task<Recruitment?> GetByIdAsync(int id);
        Task CreateAsync(Recruitment recruitment);
        Task UpdateAsync(Recruitment recruitment);
        Task DeleteAsync(int id);
    }
}