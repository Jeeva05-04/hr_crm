using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IRecruitmentRepository
    {
        Task<List<Recruitment>> GetAllAsync();
        Task AddAsync(Recruitment recruitment);
    }
}
