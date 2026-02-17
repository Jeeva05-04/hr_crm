using hr_crm.Entities;

namespace hr_crm.Services
{
    public interface IRecruitmentService
    {
        Task<List<Recruitment>> GetAllAsync();
        Task CreateAsync(Recruitment recruitment);
    }
}
